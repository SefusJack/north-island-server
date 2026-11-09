using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NorthIsland.Protocol.IO;
using NorthIsland.Protocol.Messages.Login;
using NorthIsland.Protocol.Messages.World;
using NorthIsland.Protocol.Opcodes;

namespace NorthIsland.Server.World;

/// <summary>Handles the in-world packets: walking, chat, the return spot choice, map change completion and the online status.</summary>
/// <param name="world">The online world.</param>
/// <param name="ground">The ground items.</param>
/// <param name="events">The event engine.</param>
/// <param name="teams">The team handlers, for team chat.</param>
/// <param name="guilds">The guild handlers, for guild chat.</param>
/// <param name="wander">The roaming NPCs.</param>
/// <param name="tents">The tent handlers.</param>
/// <param name="castles">The castle registry.</param>
/// <param name="marks">The mark data, for the castle reward flag.</param>
/// <param name="mines">The mine encounters, started by walking.</param>
/// <param name="fields">The field encounters, started by walking.</param>
/// <param name="gm">The GM commands, tried on every chat line.</param>
/// <param name="options">The world options.</param>
/// <param name="logger">The logger.</param>
public sealed class WorldHandlers(
    WorldState world,
    GroundItems ground,
    EventEngine events,
    TeamHandlers teams,
    GuildHandlers guilds,
    NpcWander wander,
    TentHandlers tents,
    CastleRegistry castles,
    MarkData marks,
    MineEncounters mines,
    FieldEncounters fields,
    GmCommands gm,
    IOptions<WorldOptions> options,
    ILogger<WorldHandlers> logger)
{
    /// <summary>
    /// The item ids of the Radio Set, worn for world chat, and of the Transceiver, a furniture that lets a player at home
    /// chat on the world channel without SP.
    /// </summary>
    public const ushort RadioSet = 34076, Transceiver = 39070; // 34076 Radio Set, 39070 Transceiver

    /// <summary>The warp to the starter's beach (map 11016).</summary>
    public static readonly DoorWarp StartersBeach = new(11016, 500, 1000, 0); // Beach

    /// <summary>The map id of South Island, where the castle owner packet is sent on arrival.</summary>
    public const ushort SouthIsland = 60002; // South Island

    /// <summary>The warp to Carnie (map 11094).</summary>
    public static readonly DoorWarp Carnie = new(11094, 1141, 852, 0); // Carnie

    /// <summary>Handles the choice of a return spot (1 starter's beach, 2 the recorded spot, 3 Carnie) and warps the player there.</summary>
    /// <param name="session">The player's session.</param>
    /// <param name="body">The packet body after the action and sub code.</param>
    internal ValueTask OnReturnSpot(GameSession session, ReadOnlyMemory<byte> body)
    {
        if (session.State != SessionState.InWorld || session.ActiveCharacter is not { } c || body.Length < 1 || session.Battle is not null)
            return ValueTask.CompletedTask;
        DoorWarp? warp = body.Span[0] switch // choice
        {
            1 => StartersBeach, // 1: the starter's beach
            2 when c.RecordMapId != 0 => new DoorWarp(c.RecordMapId, c.RecordX, c.RecordY, 0), // 2: the recorded spot, only when one exists
            3 => Carnie, // 3: Carnie
            _ => null, // any other choice: no warp
        };
        if (warp is null)
        {
            logger.LogWarning("[{Session}] {Name}: return spot {Choice} not available", session.Id, c.Name, body.Span[0]); // choice
            return ValueTask.CompletedTask;
        }
        session.Event = null;
        world.ChangeMap(session, warp);
        return ValueTask.CompletedTask;
    }

    /// <summary>Handles the start of auto collecting.</summary>
    /// <param name="s">The player's session.</param>
    /// <param name="body">The packet body after the action and sub code.</param>
    internal ValueTask OnAutoCollect(GameSession s, ReadOnlyMemory<byte> body)
    {
        world.StartAutoCollect(s);
        return ValueTask.CompletedTask;
    }

    /// <summary>Handles a walk: checks the walk byte, stores the new position and facing, tells the world and lets mines and fields react.</summary>
    /// <param name="session">The player's session.</param>
    /// <param name="body">The packet body after the action and sub code.</param>
    internal ValueTask OnWalk(GameSession session, ReadOnlyMemory<byte> body)
    {
        var character = session.ActiveCharacter;
        if (session.State != SessionState.InWorld || character is null || session.Account is null)
            return ValueTask.CompletedTask;

        var walk = WalkRequest.Parse(body.Span);
        var expected = WalkRequest.ExpectedCheck(session.Account.LoginId, character.Level);
        if (walk.Check != expected)
            logger.LogWarning("[{Session}] walk check byte {Actual}, expected {Expected}", session.Id, walk.Check, expected);

        var (fromX, fromY) = (character.X, character.Y);
        character.X = walk.X;
        character.Y = walk.Y;
        session.Facing = walk.Direction;
        world.Walk(session, walk.Direction, walk.X, walk.Y);
        mines.OnWalk(session, fromX, fromY);
        if (session.Event is null)
            fields.OnWalk(session);
        return ValueTask.CompletedTask;
    }

    /// <summary>Charges the SP of a world chat message when the Radio Set is worn, in a battle from the fighter.</summary>
    /// <param name="session">The player's session.</param>
    /// <returns>True when the message may go out; false when the player lacks the SP.</returns>
    private bool ChargeWorldChat(GameSession session)
    {
        var cost = options.Value.WorldChatSp;
        if (cost <= 0
            || session.ActiveCharacter is not { } c
            || !c.Equipment.Exists(e => e.Slot == Gems.AccessorySlot && e.ItemId == RadioSet && !Durability.IsDestroyed(e.Wear)))
            return true;
        var fighter = session.Battle?.Players.Find(p => ReferenceEquals(p.Session, session) && !p.Left);
        var sp = fighter?.Sp ?? c.Sp;
        if (sp < cost)
        {
            var freeAtHome = session.Account is not null
                && TentHandlers.HouseMapOf(session.Account.LoginId) == c.MapId
                && c.Furniture.Exists(f => f.ItemId == Transceiver);
            if (!freeAtHome)
                logger.LogInformation("[{Session}] {Name}: world chat refused ({Sp} SP, needs {Cost})", session.Id, c.Name, sp, cost);
            return freeAtHome;
        }
        if (fighter is not null)
            fighter.Sp -= cost;
        c.Sp = Math.Max(0, c.Sp - cost);
        session.Send(NorthIsland.Protocol.Messages.Player.InnMessages.SetAttribute(Action8.AttrSp, fighter?.Sp ?? c.Sp));
        return true;
    }

    /// <summary>Handles a chat packet: tries the GM commands, then relays it by channel (guild, world, local, team or whisper).</summary>
    /// <param name="session">The player's session.</param>
    /// <param name="channel">The chat channel.</param>
    /// <param name="body">The packet body after the action and sub code.</param>
    internal async ValueTask OnChat(GameSession session, byte channel, ReadOnlyMemory<byte> body)
    {
        if (session.State != SessionState.InWorld || session.ActiveCharacter is null)
            return;

        var chat = ChatMessage.Parse(channel, body.Span);
        logger.LogInformation("[{Session}] chat {Channel}{Target} {Name}: {Text}", session.Id, channel,
            chat.TargetId is { } t ? $" to {t}" : "", session.ActiveCharacter.Name, chat.Text);
        if (await gm.TryHandleAsync(session, chat.Text).ConfigureAwait(false))
            return;

        switch (channel)
        {
            case Action2.Guild: // 2/guild: relayed to the guild
                guilds.Chat(session, channel, chat.Text);
                break;
            case Action2.World: // 2/world: charged, then sent to all players
                if (!ChargeWorldChat(session))
                    break;
                world.Chat(session, channel, sameMapOnly: false, chat.Text);
                break;
            case Action2.Local: // 2/local: sent to the players on the same map
                world.Chat(session, channel, sameMapOnly: true, chat.Text);
                break;
            case Action2.Team: // 2/team: sent to the team
                teams.Chat(session, channel, chat.Text);
                break;
            case Action2.Whisper when chat.TargetId is { } target: // 2/whisper: refused if the target blocks them, else sent to the target
                if (world.SessionOf(target) is { } whisperTarget && BlocklistHandlers.Blocks(whisperTarget.ActiveCharacter, session.Account!.LoginId))
                {
                    BlocklistHandlers.Refuse(session, 1);
                    break;
                }
                if (!world.Whisper(session, target, channel, chat.Text))
                    logger.LogInformation("[{Session}] whisper target {Target} is not online", session.Id, target);
                break;
            default: // any other channel: logged, not relayed
                logger.LogInformation("[{Session}] chat channel {Channel} not relayed yet", session.Id, channel);
                break;
        }
    }

    /// <summary>
    /// Handles the client's report that it loaded the new map: marks the arrival and sends the ground items, tent state,
    /// roaming NPC positions and map events.
    /// </summary>
    /// <param name="session">The player's session.</param>
    /// <param name="body">The packet body after the action and sub code.</param>
    internal ValueTask OnMapChangeDone(GameSession session, ReadOnlyMemory<byte> body)
    {
        if (session.State == SessionState.InWorld && session.ActiveCharacter is not null)
        {
            session.ArrivedUtc = GameClock.UtcNow;
            world.MapLoaded(session);
            if (session.ActiveCharacter.MapId == SouthIsland)
                session.Send(castles.OwnerPacket());
            if (session.ActiveCharacter.MapId == CastleRegistry.RewardRoom
                && castles.TakeWeeklyReward(session.Account!.LoginId, session.ActiveCharacter, GameClock.Now))
            {
                marks.TrySetFlag(session, session.ActiveCharacter, CastleRegistry.RewardFlag, on: true);
                logger.LogInformation("[{Session}] {Name}: the castle's weekly reward flag", session.Id, session.ActiveCharacter.Name);
            }
            ground.SendList(session);
            tents.MapEntered(session);
            wander.SendPositions(session);
            events.EnterMap(session);
        }
        return ValueTask.CompletedTask;
    }

    /// <summary>Stores the friend list status (0 to 3) of the player and tells everyone online.</summary>
    /// <param name="session">The player's session.</param>
    /// <param name="body">The packet body after the action and sub code.</param>
    internal ValueTask OnOnlineStatus(GameSession session, ReadOnlyMemory<byte> body)
    {
        if (session.ActiveCharacter is not { } character || session.Account is null || body.Length < 1)
            return ValueTask.CompletedTask;
        var status = Math.Min((byte)3, new PacketReader(body.Span).U8());
        character.IcqStatus = status;
        world.Broadcast(session, new PacketWriter(Action5.Code, Action5.StatusChanged).I32(session.Account.LoginId).U8(status).ToArray(), sameMapOnly: false);
        logger.LogInformation("[{Session}] friend-list status {Status}", session.Id, status);
        return ValueTask.CompletedTask;
    }

    /// <summary>Logs the server address the client reports seeing.</summary>
    /// <param name="session">The player's session.</param>
    /// <param name="body">The packet body after the action and sub code.</param>
    internal ValueTask OnServerIpEcho(GameSession session, ReadOnlyMemory<byte> body)
    {
        logger.LogInformation("[{Session}] client sees server at {Address}", session.Id, ServerIpEcho.Parse(body.Span).Address);
        return ValueTask.CompletedTask;
    }

    /// <summary>Accepts a packet and ignores it.</summary>
    /// <param name="session">The player's session.</param>
    /// <param name="body">The packet body after the action and sub code.</param>
    internal static ValueTask Accept(GameSession session, ReadOnlyMemory<byte> body) => ValueTask.CompletedTask;
}
