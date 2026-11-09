using System.Collections.Frozen;
using Microsoft.Extensions.Logging;
using NorthIsland.Protocol.IO;
using NorthIsland.Protocol.Messages.Items;
using NorthIsland.Protocol.Opcodes;

namespace NorthIsland.Server.Features.Items;

/// <summary>Show items: fireworks that everyone on the map sees, and items that are thrown at a point of the map.</summary>
/// <param name="world">The online players, to show the effect to the map.</param>
/// <param name="logger">Logger.</param>
public sealed class ShowItemHandlers(
    WorldState world,
    ILogger<ShowItemHandlers> logger)
{
    /// <summary>The item sub type of the Love Fireworks.</summary>
    public const byte LoveFireworks = 143;

    /// <summary>The item sub type of the year and Spring fireworks.</summary>
    public const byte YearFireworks = 144;

    /// <summary>How long a firework runs; a player cannot light another one meanwhile.</summary>
    public static readonly TimeSpan FireworkTime = TimeSpan.FromMilliseconds(2800);

    /// <summary>The throw kind of each throwable item.</summary>
    private static readonly FrozenDictionary<ushort, byte> ThrowKinds = new Dictionary<ushort, byte>
    {
        [34251] = 1, [34261] = 1, // 34251 Water Ball, 34261 Summer Water Ball
        [34273] = 2, [34324] = 2, [34393] = 2, // 34273 Christmas Snow Ball, 34324 Happy Xmas Snowball, 34393 Happy Xmas Snowball
        [34291] = 3, // Lantern Cannon
    }.ToFrozenDictionary();

    /// <summary>The effect kind and variant a firework item shows.</summary>
    /// <param name="itemId">The item id.</param>
    /// <param name="subType">The item sub type.</param>
    /// <returns>The effect kind and its variant.</returns>
    public static (byte Kind, byte Variant) ShowOf(ushort itemId, byte subType)
    {
        if (subType == LoveFireworks)
            return (4, 1);
        return itemId switch
        {
            34235 => (2, 1), // Spring Fireworks
            34275 => (1, 2), // 2009 Fireworks
            34389 => (1, 3), // 2011 Fireworks
            34414 => (1, 4), // 2012 Fireworks
            34418 => (1, 2), // 2014 Fireworks
            _ => (1, 1), // any other firework: kind 1, variant 1
        };
    }

    /// <summary>Lights a firework: shows it to the map and uses up one item, unless the player is in a battle or one is still running.</summary>
    /// <param name="session">The player's session.</param>
    /// <param name="c">The player's character.</param>
    /// <param name="stack">The bag stack that was used.</param>
    /// <param name="item">The definition of the item.</param>
    /// <returns>True when the item is a firework and was handled; false for any other item.</returns>
    public bool TryFirework(GameSession session, Character c, BagItem stack, NorthIsland.GameData.Items.ItemRecord item)
    {
        if (item.SubType is not (LoveFireworks or YearFireworks))
            return false;
        if (session.Battle is not null || GameClock.UtcNow < session.ShowItems.FireworkUntil)
            return true;
        session.ShowItems.FireworkUntil = GameClock.UtcNow + FireworkTime;
        var (kind, variant) = ShowOf(item.Id, item.SubType);
        var packet = new PacketWriter(Action23.Code, Action23.FireworkShow).I32(session.Account!.LoginId).U8(kind).U8(variant).ToArray();
        session.Send(packet);
        world.Broadcast(session, packet, sameMapOnly: true);
        stack.Count--;
        session.Send(ItemMessages.BagSlotDecrease(stack.Slot, 1));
        if (stack.Count == 0)
            c.Bag.Remove(stack);
        session.Send(ItemMessages.UsedSound());
        logger.LogInformation("[{Session}] {Name} lights {Item}: kind {Kind}/{Variant}", session.Id, c.Name, item.Name.Trim(), kind, variant);
        return true;
    }

    /// <summary>Handles throwing an item at a point of the map: shows the throw to the map and uses up one item.</summary>
    /// <param name="session">The player's session.</param>
    /// <param name="body">The packet body: the bag slot and the x and y map pixels.</param>
    internal ValueTask OnThrow(GameSession session, ReadOnlyMemory<byte> body)
    {
        if (session.State != SessionState.InWorld || session.ActiveCharacter is not { } c || body.Length < 5 || session.Battle is not null)
            return ValueTask.CompletedTask;
        var r = new PacketReader(body.Span);
        var (slot, x, y) = (r.U8(), r.U16(), r.U16());
        var stack = c.Bag.Find(b => b.Slot == slot);
        if (stack is null || !ThrowKinds.TryGetValue(stack.ItemId, out var kind))
        {
            logger.LogInformation("[{Session}] {Name}: bag slot {Slot} holds nothing to throw", session.Id, c.Name, slot);
            return ValueTask.CompletedTask;
        }
        var packet = new PacketWriter(Action23.Code, Action23.Throw).U8(kind).I32(session.Account!.LoginId).U16(x).U16(y).ToArray();
        session.Send(packet);
        world.Broadcast(session, packet, sameMapOnly: true);
        stack.Count--;
        session.Send(ItemMessages.BagSlotDecrease(slot, 1));
        if (stack.Count == 0)
            c.Bag.Remove(stack);
        return ValueTask.CompletedTask;
    }
}
