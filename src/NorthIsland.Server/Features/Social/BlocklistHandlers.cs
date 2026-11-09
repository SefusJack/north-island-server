using System.Buffers.Binary;
using Microsoft.Extensions.Logging;
using NorthIsland.Protocol.Messages.Player;
using NorthIsland.Protocol.Opcodes;
using NorthIsland.Protocol.Text;

namespace NorthIsland.Server.Features.Social;

/// <summary>Keeps each character's block list and answers the block list packets.</summary>
/// <param name="world">World state, to look up online players.</param>
/// <param name="logger">Logger.</param>
public sealed class BlocklistHandlers(
    WorldState world,
    ILogger<BlocklistHandlers> logger)
{
    /// <summary>Most names a block list can hold.</summary>
    public const int MaxEntries = 20;

    /// <summary>Tells whether a character has a player on their block list.</summary>
    /// <param name="target">The character who may block; may be null.</param>
    /// <param name="senderId">The login id of the player who acts.</param>
    /// <returns>True when the sender is on the target's list.</returns>
    public static bool Blocks(Character? target, int senderId) => target is not null && target.Blocked.Exists(b => b.Id == senderId);

    /// <summary>Tells a player that their action was refused because they are blocked.</summary>
    /// <param name="sender">The session of the refused player.</param>
    /// <param name="action">The number of the refused action.</param>
    public static void Refuse(GameSession sender, byte action) => sender.Send(FriendMessages.BlockedBy(action));

    /// <summary>Sends the block list to a player on login when it is not empty.</summary>
    /// <param name="session">The player's session.</param>
    /// <param name="character">The player's character.</param>
    public static void SendOnLogin(GameSession session, Character character)
    {
        if (character.Blocked.Count > 0)
            session.Send(FriendMessages.Blocklist(Action14.BlocklistFull, character.Blocked.Select(b => (b.Id, b.Name))));
    }

    /// <summary>Handles a block list request: add, remove or send the whole list.</summary>
    /// <param name="session">The player's session.</param>
    /// <param name="body">The packet body: the operation (1 add, 2 remove, 3 send the list); for 1 and 2 a login id and a length-prefixed name.</param>
    internal ValueTask OnBlocklist(GameSession session, ReadOnlyMemory<byte> body)
    {
        if (session.State != SessionState.InWorld || session.ActiveCharacter is not { } c || body.Length < 1)
            return ValueTask.CompletedTask;
        var op = body.Span[0];
        if (op == 3)
        {
            session.Send(FriendMessages.Blocklist(Action14.BlocklistFull, c.Blocked.Select(b => (b.Id, b.Name))));
            return ValueTask.CompletedTask;
        }
        if (op is not (1 or 2) || body.Length < 6)
            return ValueTask.CompletedTask;
        var id = BinaryPrimitives.ReadInt32LittleEndian(body.Span[1..]);
        var nameLength = body.Length > 5 ? body.Span[5] : 0;
        var name = body.Length >= 6 + nameLength ? GameEncoding.GetString(body.Span.Slice(6, nameLength)) : "";
        var myId = session.Account!.LoginId;
        if (op == 1)
        {
            if (id == myId || c.Blocked.Count >= MaxEntries || c.Blocked.Exists(b => b.Id == id) || name.Length == 0)
            {
                Fail(session, c);
                return ValueTask.CompletedTask;
            }
            if (world.SessionOf(id) is { } other && other.ActiveCharacter is { } named)
                name = named.Name;
            c.Blocked.Add(new BlockedPlayer { Id = id, Name = name });
            session.Send(FriendMessages.Blocklist(Action14.BlocklistAdded, []));
            logger.LogInformation("[{Session}] {Name} blocks {Id} {Other}", session.Id, c.Name, id, name);
        }
        else
        {
            if (c.Blocked.RemoveAll(b => b.Id == id) == 0)
            {
                Fail(session, c);
                return ValueTask.CompletedTask;
            }
            session.Send(FriendMessages.Blocklist(Action14.BlocklistRemoved, []));
            logger.LogInformation("[{Session}] {Name} unblocks {Id}", session.Id, c.Name, id);
        }
        return ValueTask.CompletedTask;
    }

    /// <summary>Answers a failed add or remove with the current list.</summary>
    /// <param name="session">The player's session.</param>
    /// <param name="c">The player's character.</param>
    private static void Fail(GameSession session, Character c) =>
        session.Send(FriendMessages.Blocklist(Action14.BlocklistFailed, c.Blocked.Select(b => (b.Id, b.Name))));
}
