using System.Collections.Immutable;
using Microsoft.Extensions.Logging;
using NorthIsland.GameData.Items;
using NorthIsland.Protocol.IO;
using NorthIsland.Protocol.Messages.Items;
using NorthIsland.Protocol.Messages.Player;
using NorthIsland.Protocol.Opcodes;

namespace NorthIsland.Server.Features.Houses;

/// <summary>The Fish Urn in a player's house: shows the tank, lets the owner buy and throw away fish and feed it.</summary>
/// <param name="items">The item data, for names.</param>
/// <param name="tents">The house and furniture handlers, for house owners.</param>
/// <param name="logger">The logger.</param>
public sealed class FishTankHandlers(
    ItemData items,
    TentHandlers tents,
    ILogger<FishTankHandlers> logger)
{
    /// <summary>The item ids of the Fish Urn and of the fish food.</summary>
    public const ushort FishUrn = 39099, FishFood = 34161; // 39099 Fish Urn, 34161 Fresh Fish Food

    /// <summary>The number of fish slots in the tank and the most pieces of food it holds.</summary>
    public const int Slots = 8, MaxFood = 250;

    /// <summary>The price in gold of a fish, by kind; kinds 0 to 2 have none.</summary>
    private static readonly ImmutableArray<int> Prices = [0, 0, 0, 50, 60, 10, 100, 150];

    /// <summary>Builds a fish tank packet <c>23/160</c> that has only a sub code.</summary>
    /// <param name="sub">The sub code.</param>
    /// <returns>The packet.</returns>
    private static byte[] Tank(byte sub) => new PacketWriter(Action23.Code, Action23.FishTank).U8(sub).ToArray();

    /// <summary>Builds a fish tank packet <c>23/160</c> with one value byte.</summary>
    /// <param name="sub">The sub code.</param>
    /// <param name="a">The value byte.</param>
    /// <returns>The packet.</returns>
    private static byte[] Tank(byte sub, byte a) => new PacketWriter(Action23.Code, Action23.FishTank).U8(sub).U8(a).ToArray();

    /// <summary>How many fish the character bought.</summary>
    /// <param name="c">The character.</param>
    /// <returns>The number of filled tank slots.</returns>
    private static int Bought(Character c) => c.FishTank.Count(k => k != 0);

    /// <summary>The active character of a session that is in the world and stands in their own house with a Fish Urn placed.</summary>
    /// <param name="session">The session.</param>
    /// <returns>The character, or null when the conditions do not hold.</returns>
    private static Character? InOwnHouse(GameSession session)
    {
        var character = session.ActiveCharacter;
        return session.State == SessionState.InWorld && character is not null && session.Account is not null
            && TentHandlers.HouseMapOf(session.Account.LoginId) == character.MapId && character.Furniture.Exists(f => f.ItemId == FishUrn && !f.Retained)
            ? character
            : null;
    }

    /// <summary>Handles <c>62/56</c>: shows the tank of the house the player is in, as an owner or as a visitor.</summary>
    /// <param name="session">The session.</param>
    /// <param name="body">The packet body, not used.</param>
    internal ValueTask OnClick(GameSession session, ReadOnlyMemory<byte> body)
    {
        var me = session.ActiveCharacter;
        if (session.State != SessionState.InWorld || me is null || !TentHandlers.IsHouseMap(me.MapId))
            return ValueTask.CompletedTask;
        var owner = tents.OwnerOfHouse(me.MapId);
        if (owner is null || !owner.Character.Furniture.Exists(f => f.ItemId == FishUrn && !f.Retained))
            return ValueTask.CompletedTask;
        var host = owner.Character;
        session.Send(new PacketWriter(Action23.Code, Action23.FishTank).U8(9).Bytes(host.FishTank).ToArray());
        session.Send(new PacketWriter(Action23.Code, Action23.FishTank).U8(11).U8((byte)Bought(host)).U8(host.FishFood).ToArray());
        session.Send(Tank(owner.Session == session ? (byte)1 : (byte)3)); // sub
        return ValueTask.CompletedTask;
    }

    /// <summary>Handles <c>23/97</c>: the owner feeds the fish one piece of food from the tank's supply.</summary>
    /// <param name="session">The owner's session.</param>
    /// <param name="body">The request byte; only 2 (feed) is acted on.</param>
    internal static ValueTask OnFeed(GameSession session, ReadOnlyMemory<byte> body)
    {
        if (InOwnHouse(session) is not { } c || body.Length < 1 || body.Span[0] != 2) // request
            return ValueTask.CompletedTask;
        if (c.FishFood == 0)
        {
            session.Send(Tank(4)); // sub
            return ValueTask.CompletedTask;
        }
        c.FishFood--;
        session.Send(Tank(2, c.FishFood)); // sub
        return ValueTask.CompletedTask;
    }

    /// <summary>Handles <c>23/98</c>: the owner asks for the buy list or the throw-away list, buys a fish or throws one away.</summary>
    /// <param name="session">The owner's session.</param>
    /// <param name="body">The request byte, then the fish kind for buying and throwing away.</param>
    internal ValueTask OnRequest(GameSession session, ReadOnlyMemory<byte> body)
    {
        if (InOwnHouse(session) is not { } c || body.Length < 1)
            return ValueTask.CompletedTask;
        var kind = body.Length > 1 ? body.Span[1] : (byte)0;
        switch (body.Span[0]) // request
        {
            case 3: // 23/98/3: send the buy list
                session.Send(Tank(8)); // sub
                break;
            case 4: // 23/98/4: send the throw-away list when fish were bought
                if (Bought(c) > 0)
                    session.Send(Tank(10)); // sub
                break;
            case 2 when kind is >= 3 and <= 7: // 23/98/2: buy a fish of that kind
                Buy(session, c, kind);
                break;
            case 5 when kind is >= 3 and <= 7: // 23/98/5: throw away a fish of that kind
                var slot = Array.IndexOf(c.FishTank, kind);
                if (slot >= 0)
                {
                    c.FishTank[slot] = 0;
                    session.Send(Tank(7, (byte)Bought(c))); // sub
                    session.Send(new PacketWriter(Action23.Code, Action23.FishTank).U8(9).Bytes(c.FishTank).ToArray());
                }
                break;
        }
        return ValueTask.CompletedTask;
    }

    /// <summary>Buys a fish of a kind into the first free tank slot if the owner has the gold.</summary>
    /// <param name="session">The owner's session.</param>
    /// <param name="c">The owner's character.</param>
    /// <param name="kind">The fish kind, 3 to 7.</param>
    private void Buy(GameSession session, Character c, byte kind)
    {
        var slot = Array.IndexOf(c.FishTank, (byte)0);
        if (slot < 0)
        {
            session.Send(Tank(12)); // sub
            return;
        }
        if (c.Gold < Prices[kind])
            return;
        c.Gold -= Prices[kind];
        session.Send(PlayerMessages.MoneyChange(-Prices[kind]));
        c.FishTank[slot] = kind;
        session.Send(new PacketWriter(Action23.Code, Action23.FishTank).U8(6).U8(kind).U8((byte)Bought(c)).ToArray());
        logger.LogInformation("[{Session}] {Name} buys a fish of kind {Kind} for {Price}", session.Id, c.Name, kind, Prices[kind]);
    }

    /// <summary>Puts a piece of fish food from the bag into the tank when the item used is the fish food.</summary>
    /// <param name="session">The session.</param>
    /// <param name="c">The character.</param>
    /// <param name="stack">The bag stack that was used.</param>
    /// <param name="item">The record of the used item.</param>
    /// <returns>True when the item is the fish food, whether or not the food was added.</returns>
    public bool TryUseFood(GameSession session, Character c, BagItem stack, ItemRecord item)
    {
        if (item.Id != FishFood)
            return false;
        if (InOwnHouse(session) is null || c.FishFood >= MaxFood)
        {
            session.Send(Tank(4)); // sub
            return true;
        }
        c.FishFood++;
        stack.Count--;
        if (stack.Count == 0)
            c.Bag.Remove(stack);
        session.Send(ItemMessages.BagSlotDecrease(stack.Slot, 1));
        session.Send(Tank(5, c.FishFood)); // sub
        logger.LogInformation("[{Session}] {Name} puts {Item} into the fish tank ({Food} pieces)", session.Id, c.Name, items[item.Id]?.Name.Trim(), c.FishFood);
        return true;
    }
}
