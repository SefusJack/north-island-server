using Microsoft.Extensions.Logging;
using NorthIsland.GameData.Items;
using NorthIsland.Protocol.IO;
using NorthIsland.Protocol.Messages.Items;
using NorthIsland.Protocol.Messages.World;
using NorthIsland.Protocol.Opcodes;

namespace NorthIsland.Server.Features.Storage;

/// <summary>Serves the Space Contract on the wire: using the items, the state packets, the end of the contract and the house room size.</summary>
/// <param name="space">The Space Contract rules.</param>
/// <param name="mini">The Item Bag handlers, opened when the stock keeper part starts.</param>
/// <param name="world">The online sessions.</param>
/// <param name="tents">The house handlers, whose house entry triggers the expiry check.</param>
/// <param name="logger">The logger.</param>
public sealed class SpaceContractHandlers(
    SpaceContract space,
    MiniGridHandlers mini,
    WorldState world,
    TentHandlers tents,
    ILogger<SpaceContractHandlers> logger)
{
    /// <summary>Subscribes to the house entry event so an expired contract is noticed when the owner enters their house.</summary>
    internal void Start() => tents.HouseEntered += OnHouseEntered;

    /// <summary>Builds the <c>23/136</c> state packet with one state number.</summary>
    /// <param name="kind">The state number.</param>
    /// <returns>The packet.</returns>
    private static byte[] State(byte kind) => new PacketWriter(Action23.Code, Action23.SpaceContract).U8(kind).ToArray();

    /// <summary>Builds the <c>23/136</c> state packet with a state number and a number of hours.</summary>
    /// <param name="kind">The state number.</param>
    /// <param name="hours">The hours.</param>
    /// <returns>The packet.</returns>
    private static byte[] Hours(byte kind, int hours) => new PacketWriter(Action23.Code, Action23.SpaceContract).U8(kind).I32(hours).ToArray();

    /// <summary>Builds the <c>23/136/5</c> packet that announces the open parts and their hours.</summary>
    /// <param name="flags">The part flags.</param>
    /// <param name="hours">The hours left.</param>
    /// <returns>The packet.</returns>
    private static byte[] Opened(byte flags, int hours) => new PacketWriter(Action23.Code, Action23.SpaceContract).U8(5).U8(flags).I32(hours).ToArray();

    /// <summary>
    /// Uses a Space Contract or Space Agreement item from the bag: extends the contract, consumes one item and sends the new states. At the upper
    /// limit only the refusal state is sent.
    /// </summary>
    /// <param name="session">The player's session.</param>
    /// <param name="c">The player's character.</param>
    /// <param name="stack">The bag stack being used.</param>
    /// <param name="item">The record of the used item.</param>
    /// <returns>True when the item was a contract item and has been handled, false for any other item.</returns>
    public bool TryUse(GameSession session, Character c, BagItem stack, ItemRecord item)
    {
        if (SpaceContract.FlagsOf(item.Id) == 0)
            return false;
        var now = GameClock.UtcNow;
        Check(session, c);
        if (!space.Use(c, item.Id, now))
        {
            session.Send(State(10));
            return true;
        }
        stack.Count--;
        if (stack.Count == 0)
            c.Bag.Remove(stack);
        session.Send(ItemMessages.BagSlotDecrease(stack.Slot, 1));
        session.Send(Opened(c.SpaceFlags, SpaceContract.HoursLeft(c, now)));
        SendStates(session, c);
        session.Send(State(space.Has(c, SpaceContract.Garage) ? (byte)8 : (byte)9));
        if ((c.SpaceFlags & SpaceContract.Stock) != 0)
            mini.Open(session, c);
        SendRoomSize(session, c);
        logger.LogInformation(
            "[{Session}] {Name} uses {Item}: flags {Flags}, {Hours} hours left",
            session.Id,
            c.Name,
            item.Name.Trim(),
            c.SpaceFlags,
            SpaceContract.HoursLeft(c, now));
        return true;
    }

    /// <summary>Sends the states of the Item Bag and of the hotel.</summary>
    /// <param name="session">The player's session.</param>
    /// <param name="c">The player's character.</param>
    private void SendStates(GameSession session, Character c)
    {
        session.Send(State(space.Has(c, SpaceContract.Stock) ? (byte)1 : (byte)2));
        session.Send(State(space.Has(c, SpaceContract.Hotel) ? (byte)3 : (byte)4));
    }

    /// <summary>Checks the contract of a player who enters the world and sends the hours left when one runs.</summary>
    /// <param name="session">The player's session.</param>
    /// <param name="c">The player's character.</param>
    public void SendOnLogin(GameSession session, Character c)
    {
        Check(session, c);
        if (SpaceContract.Active(c, GameClock.UtcNow))
            session.Send(Hours(7, SpaceContract.HoursLeft(c, GameClock.UtcNow)));
    }

    /// <summary>
    /// Ends the contract when it has run out: sends the end and the new states, moves furniture that no longer fits the smaller house room to the
    /// pick-up area, and sends the room size.
    /// </summary>
    /// <param name="session">The player's session.</param>
    /// <param name="c">The player's character.</param>
    public void Check(GameSession session, Character c)
    {
        if (!SpaceContract.Ended(c, GameClock.UtcNow))
            return;
        session.Send(State(6));
        SendStates(session, c);
        session.Send(State(9));
        var size = space.TentSize(c);
        foreach (var piece in c.Furniture.Where(f => f.Parent == 0 && !f.Retained && f.Room <= 1 && f.Y < SpaceContract.TopRow(size)).ToList())
        {
            (piece.Retained, piece.X, piece.Y, piece.Z) = (true, 0, 0, 0);
            if (OwnHouse(session, c))
                session.Send(HouseMessages.FurnitureRetained(piece.Index, true));
        }
        SendRoomSize(session, c);
        logger.LogInformation("[{Session}] {Name}: the Space Contract is over", session.Id, c.Name);
    }

    /// <summary>Tells whether the character stands in their own house.</summary>
    /// <param name="session">The player's session.</param>
    /// <param name="c">The player's character.</param>
    /// <returns>True when the map is the player's own house.</returns>
    private static bool OwnHouse(GameSession session, Character c) =>
        session.Account is not null && TentHandlers.IsHouseMap(c.MapId) && TentHandlers.HouseMapOf(session.Account.LoginId) == c.MapId;

    /// <summary>Sends the house room size to everyone in the owner's house room, when the character is in their own house.</summary>
    /// <param name="session">The player's session.</param>
    /// <param name="c">The player's character.</param>
    private void SendRoomSize(GameSession session, Character c)
    {
        if (!OwnHouse(session, c) || session.HouseRoom > 1)
            return;
        var size = space.TentSize(c);
        foreach (var s in world.OnMap(c.MapId).Append(session).Distinct().Where(s => s.HouseRoom <= 1))
            s.Send(HouseMessages.RoomSize(size, s.HouseRoom));
    }

    /// <summary>Checks the contract when the owner enters their own house.</summary>
    /// <param name="session">The player's session.</param>
    /// <param name="owner">The owner of the house that was entered.</param>
    private void OnHouseEntered(GameSession session, GameSession? owner)
    {
        if (owner == session && session.ActiveCharacter is { } c)
            Check(session, c);
    }
}
