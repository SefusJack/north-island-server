using Microsoft.Extensions.Logging;
using NorthIsland.Protocol.Messages.Items;

namespace NorthIsland.Server.Features.Items;

/// <summary>
/// Riceball body shifting: using a Riceball turns the player into an NPC shape for two hours, shown to everyone on the map, and the client can
/// take the shape off and put it on again while time is left.
/// </summary>
/// <param name="items">Item definitions, to read the shape of a Riceball.</param>
/// <param name="npcs">NPC definitions, to check that the shape exists.</param>
/// <param name="world">The online players, to show the shape to other sessions.</param>
/// <param name="logger">Logger.</param>
public sealed class BodyShiftHandlers(
    ItemData items,
    NpcData npcs,
    WorldState world,
    ILogger<BodyShiftHandlers> logger)
{
    /// <summary>The item sub type of a Riceball.</summary>
    public const byte RiceballSubType = 66;

    /// <summary>How long a Riceball shape lasts once it is used.</summary>
    public static readonly TimeSpan Duration = TimeSpan.FromHours(2);

    /// <summary>Hooks the world so a player who comes into view of a shifted player is sent that player's NPC shape.</summary>
    internal void Start()
    {
        world.PlayersMet += (a, b) =>
        {
            if (a.ActiveCharacter is { BodyShiftOn: true } ca)
                b.Send(ItemMessages.Transform([(a.Account!.LoginId, ca.BodyShiftNpc)]));
            if (b.ActiveCharacter is { BodyShiftOn: true } cb)
                a.Send(ItemMessages.Transform([(b.Account!.LoginId, cb.BodyShiftNpc)]));
        };
    }

    /// <summary>The NPC id a Riceball turns the player into.</summary>
    /// <param name="item">The item definition.</param>
    /// <returns>The NPC id, or 0 when the item is not a Riceball or names no shape.</returns>
    public static ushort ShapeOf(NorthIsland.GameData.Items.ItemRecord item) =>
        item.SubType == RiceballSubType && item.Words415.Length > 2 ? item.Words415[2] : (ushort)0;

    /// <summary>The seconds of shape time left: running down while the shape is on, frozen while it is off.</summary>
    /// <param name="c">The player's character.</param>
    /// <param name="now">The current time.</param>
    /// <returns>The seconds left, never negative while the shape is on.</returns>
    private static int SecondsLeft(Character c, DateTime now) =>
        c.BodyShiftOn ? Math.Max(0, (int)Math.Ceiling((c.BodyShiftEnds - now).TotalSeconds)) : c.BodyShiftLeft;

    /// <summary>Sends the NPC shape (or 0 for their own look) to the player and to the others on their map.</summary>
    /// <param name="session">The player's session.</param>
    /// <param name="npc">The NPC id to show, 0 to show the player again.</param>
    private void Show(GameSession session, ushort npc)
    {
        var packet = ItemMessages.Transform([(session.Account!.LoginId, npc)]);
        session.Send(packet);
        world.Broadcast(session, packet, sameMapOnly: true);
    }

    /// <summary>
    /// Handles the body shifting action: sub code 1 uses a Riceball from a bag slot, 2 asks for the time left, 3 puts the stored shape on again and
    /// 4 takes it off.
    /// </summary>
    /// <param name="session">The player's session.</param>
    /// <param name="body">The packet body after the action and sub code.</param>
    internal ValueTask OnBodyShift(GameSession session, ReadOnlyMemory<byte> body)
    {
        if (session.State != SessionState.InWorld || session.ActiveCharacter is not { } c || body.Length < 1)
            return ValueTask.CompletedTask;
        var now = GameClock.UtcNow;
        switch (body.Span[0])
        {
            case 1 when body.Length >= 2: // 23/114/1: use a Riceball from a bag slot
                Use(session, c, body.Span[1], now);
                break;
            case 2: // 23/114/2: ask for the time left
                if (SecondsLeft(c, now) > 0)
                    session.Send(ItemMessages.BodyShiftAsk((ushort)Math.Min(ushort.MaxValue, (SecondsLeft(c, now) + 59) / 60)));
                break;
            case 3: // 23/114/3: put the stored shape on again
                if (c.BodyShiftNpc == 0 || SecondsLeft(c, now) <= 0 || CannotNow(session, c))
                    session.Send(ItemMessages.BodyShiftRefused(3)); // reason
                else if (c.BodyShiftOn)
                    session.Send(ItemMessages.BodyShiftRefused(2)); // reason
                else
                {
                    c.BodyShiftOn = true;
                    c.BodyShiftEnds = now.AddSeconds(c.BodyShiftLeft);
                    Show(session, c.BodyShiftNpc);
                    session.Send(ItemMessages.BodyShiftState(1));
                }
                break;
            case 4: // 23/114/4: take the shape off
                if (!c.BodyShiftOn)
                    session.Send(ItemMessages.BodyShiftRefused(2)); // reason
                else
                    TakeOff(session, c, now, timeIsUp: false);
                break;
        }
        return ValueTask.CompletedTask;
    }

    /// <summary>Whether the player is somewhere a shape cannot be put on: in a battle, in a bath, riding a pet, or on one of two maps.</summary>
    /// <param name="session">The player's session.</param>
    /// <param name="c">The player's character.</param>
    /// <returns>True when the shape cannot be used now.</returns>
    private static bool CannotNow(GameSession session, Character c) =>
        session.Battle is not null || session.HouseLife.BathFurn != 0 || session.RidingPetSlot != 0
            || c.MapId is 10085 // Wedding Hall
                or 10079; // Quiz Zone

    /// <summary>
    /// Uses the Riceball in a bag slot: takes one from the stack, starts the two hours and shows the shape, or refuses when the
    /// item gives no shape or the player cannot shift now.
    /// </summary>
    /// <param name="session">The player's session.</param>
    /// <param name="c">The player's character.</param>
    /// <param name="slot">The bag slot of the Riceball.</param>
    /// <param name="now">The current time.</param>
    private void Use(GameSession session, Character c, byte slot, DateTime now)
    {
        var stack = c.Bag.Find(b => b.Slot == slot);
        var npc = stack is null || items[stack.ItemId] is not { } item ? (ushort)0 : ShapeOf(item);
        if (stack is null || npc == 0 || npcs[npc] is null || CannotNow(session, c))
        {
            logger.LogInformation("[{Session}] {Name}: bag slot {Slot} cannot be used as a Riceball now", session.Id, c.Name, slot);
            session.Send(ItemMessages.BodyShiftRefused(3)); // reason
            return;
        }
        stack.Count--;
        session.Send(ItemMessages.BagSlotDecrease(slot, 1));
        if (stack.Count == 0)
            c.Bag.Remove(stack);
        session.Send(ItemMessages.UsedSound());
        c.BodyShiftNpc = npc;
        c.BodyShiftOn = true;
        c.BodyShiftEnds = now + Duration;
        Show(session, npc);
        session.Send(ItemMessages.BodyShiftState(1));
        logger.LogInformation("[{Session}] {Name} turns into npc {Npc} for {Hours} hours", session.Id, c.Name, npc, Duration.TotalHours);
    }

    /// <summary>Takes the shape off, keeping the time left unless it ran out, and shows the player's own look again.</summary>
    /// <param name="session">The player's session.</param>
    /// <param name="c">The player's character.</param>
    /// <param name="now">The current time.</param>
    /// <param name="timeIsUp">True when the shape ends because its time ran out, false when the player took it off.</param>
    private void TakeOff(GameSession session, Character c, DateTime now, bool timeIsUp)
    {
        c.BodyShiftLeft = timeIsUp ? 0 : SecondsLeft(c, now);
        c.BodyShiftOn = false;
        if (c.BodyShiftLeft <= 0)
            c.BodyShiftNpc = 0;
        Show(session, 0);
        session.Send(ItemMessages.BodyShiftState((byte)(timeIsUp ? 0 : 2)));
    }

    /// <summary>At login, ends a shape whose time passed while offline, or restores the shape that was on, or tells the client that time is stored.</summary>
    /// <param name="session">The player's session.</param>
    /// <param name="c">The player's character.</param>
    public void SendOnLogin(GameSession session, Character c)
    {
        var now = GameClock.UtcNow;
        if (c.BodyShiftOn && c.BodyShiftEnds <= now)
        {
            c.BodyShiftOn = false;
            c.BodyShiftNpc = 0;
            c.BodyShiftLeft = 0;
            session.Send(ItemMessages.BodyShiftState(0));
        }
        else if (c.BodyShiftOn)
        {
            Show(session, c.BodyShiftNpc);
            session.Send(ItemMessages.BodyShiftState(1));
        }
        else if (c.BodyShiftLeft > 0)
            session.Send(ItemMessages.BodyShiftState(3)); // state
    }

    /// <summary>Takes the shape off when its time has run out.</summary>
    /// <param name="session">The player's session.</param>
    /// <param name="c">The player's character.</param>
    /// <param name="now">The current time.</param>
    public void Tick(GameSession session, Character c, DateTime now)
    {
        if (c.BodyShiftOn && c.BodyShiftEnds <= now)
            TakeOff(session, c, now, timeIsUp: true);
    }

    /// <summary>The sessions that are online, for the ticker to walk.</summary>
    /// <returns>The online sessions.</returns>
    public IEnumerable<GameSession> Online() => world.OnlineSessions();
}
