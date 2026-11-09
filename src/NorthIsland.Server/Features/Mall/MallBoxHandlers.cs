using System.Collections.Immutable;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NorthIsland.Protocol.Messages.Items;
using NorthIsland.Protocol.Messages.Player;

namespace NorthIsland.Server.Features.Mall;

/// <summary>Handles opening boxes, lucky bags and gift boxes from the bag, and the lucky bag content preview.</summary>
/// <param name="items">Item definitions.</param>
/// <param name="options">Mall options; names the lucky bag override file.</param>
/// <param name="logger">Logger.</param>
public sealed class MallBoxHandlers(
    ItemData items,
    IOptions<MallOptions> options,
    ILogger<MallBoxHandlers> logger)
{
    /// <summary>Reward tables by box item id.</summary>
    private readonly IReadOnlyDictionary<ushort, IReadOnlyList<MallBoxEntry>> _tables = MallBoxes.Load(
        options.Value.MallBoxes.LuckyBagsFile is { Length: > 0 } f ? ServerPaths.Resolve(f) : null);

    /// <summary>Fixed contents by gift box item id.</summary>
    private readonly IReadOnlyDictionary<ushort, GiftBox> _gifts = GiftBoxes.Load();

    /// <summary>Item id of the Purple Chest, which needs a Purple Chest Key to open.</summary>
    public const ushort PurpleChest = 34268; // Purple Chest

    /// <summary>Item ids of the keys that open the Purple Chest.</summary>
    public static readonly ImmutableArray<ushort> PurpleKeys = [30334, 30371]; // 30334 Purple Chest Key, 30371 Purple Chest Key

    /// <summary>
    /// Answers a lucky bag preview request with the bag's reward list, or without a list when the client's count already matches the table.
    /// </summary>
    /// <param name="session">The player's session.</param>
    /// <param name="body">The packet body after the action and sub code.</param>
    internal ValueTask OnPreview(GameSession session, ReadOnlyMemory<byte> body)
    {
        if (session.State != SessionState.InWorld || body.Length < 3)
            return ValueTask.CompletedTask;
        var bagId = System.Buffers.Binary.BinaryPrimitives.ReadUInt16LittleEndian(body.Span);
        if (!_tables.TryGetValue(bagId, out var table) || items[bagId] is not { Type: 41 })
            return ValueTask.CompletedTask;
        session.Send(LuckyBagMessages.Info(bagId, body.Span[2] == table.Count ? null : [.. table.Select(e => (e.ItemId, e.Count))]));
        return ValueTask.CompletedTask;
    }

    /// <summary>Opens a gift box in the bag: takes one box and gives its fixed items and gold, provided everything fits.</summary>
    /// <param name="session">The player's session.</param>
    /// <param name="body">The packet body after the action and sub code.</param>
    internal ValueTask OnOpenGift(GameSession session, ReadOnlyMemory<byte> body)
    {
        if (session.State != SessionState.InWorld || session.ActiveCharacter is not { } c || session.Battle is not null || body.Length < 1)
            return ValueTask.CompletedTask;
        var stack = c.Bag.Find(b => b.Slot == body.Span[0]);
        if (stack is null || !_gifts.TryGetValue(stack.ItemId, out var box))
        {
            logger.LogInformation("[{Session}] {Name}: bag slot {Slot} is not a gift box with known contents", session.Id, c.Name, body.Span[0]);
            return ValueTask.CompletedTask;
        }
        var trial = c.Bag.Select(
            b =>
                new BagItem
                {
                    Slot = b.Slot,
                    ItemId = b.ItemId,
                    Count = b.Count,
                    Wear = b.Wear,
                    Insert = b.Insert,
                    Fusion = b.Fusion,
                    Pose = b.Pose,
                    BoxItem = b.BoxItem
                })
            .ToList();
        Bag.Remove(trial, stack.ItemId, 1);
        if (box.Items.Any(i => items[i.Item] is null || Bag.Add(trial, items, i.Item, i.Count).Sum(x => x.Added) != i.Count)
            || (long)c.Gold + box.Gold > int.MaxValue)
        {
            logger.LogInformation("[{Session}] {Name}: no room for the contents of {Box}", session.Id, c.Name, items[stack.ItemId]!.Name.Trim());
            return ValueTask.CompletedTask;
        }
        var name = items[stack.ItemId]!.Name.Trim();
        stack.Count--;
        if (stack.Count == 0)
            c.Bag.Remove(stack);
        session.Send(ItemMessages.BagSlotDecrease(body.Span[0], 1));
        foreach (var (item, count) in box.Items)
            foreach (var (added, n) in Bag.Add(c, items, item, count))
                session.Send(ItemMessages.BagAdd(new BagEntry { Slot = added.Slot, ItemId = added.ItemId, Count = n, Wear = added.Wear }));
        if (box.Gold > 0)
        {
            c.Gold += box.Gold;
            session.Send(PlayerMessages.MoneyChange(box.Gold));
        }
        logger.LogInformation("[{Session}] {Name} opens {Box}: {Items} item stack(s), {Gold} gold", session.Id, c.Name, name, box.Items.Count, box.Gold);
        return ValueTask.CompletedTask;
    }

    /// <summary>
    /// Opens a box from a bag slot: rolls one reward by weight, takes one box (and one key for the Purple Chest) and gives the reward when it fits.
    /// </summary>
    /// <param name="session">The player's session.</param>
    /// <param name="body">The packet body after the action and sub code.</param>
    internal ValueTask OnOpen(GameSession session, ReadOnlyMemory<byte> body)
    {
        if (session.State != SessionState.InWorld || session.ActiveCharacter is not { } c || session.Battle is not null || body.Length < 1)
            return ValueTask.CompletedTask;
        var stack = c.Bag.Find(b => b.Slot == body.Span[0]);
        if (stack is null || !_tables.TryGetValue(stack.ItemId, out var table))
        {
            logger.LogInformation("[{Session}] {Name}: bag slot {Slot} does not open as a box", session.Id, c.Name, body.Span[0]);
            return ValueTask.CompletedTask;
        }
        var reward = MallBoxes.Roll(table, GameRandom.Shared.Next);
        if (items[reward.ItemId] is null)
            return ValueTask.CompletedTask;
        BagItem? key = null;
        if (stack.ItemId == PurpleChest)
        {
            key = body.Length >= 2 ? c.Bag.Find(b => b.Slot == body.Span[1]) : null;
            if (key is null || PurpleKeys.IndexOf(key.ItemId) < 0)
            {
                logger.LogInformation("[{Session}] {Name}: the Purple Chest needs a Purple Chest Key", session.Id, c.Name);
                return ValueTask.CompletedTask;
            }
        }
        var trial = c.Bag.Select(
            b =>
                new BagItem
                {
                    Slot = b.Slot,
                    ItemId = b.ItemId,
                    Count = b.Count,
                    Wear = b.Wear,
                    Insert = b.Insert,
                    Fusion = b.Fusion,
                    Pose = b.Pose,
                    BoxItem = b.BoxItem
                })
            .ToList();
        Bag.Remove(trial, stack.ItemId, 1);
        if (Bag.Add(trial, items, reward.ItemId, reward.Count).Sum(x => x.Added) != reward.Count)
        {
            logger.LogInformation(
                "[{Session}] {Name}: no room for {Item} x{Count} from {Box}",
                session.Id,
                c.Name,
                items[reward.ItemId]!.Name.Trim(),
                reward.Count,
                items[stack.ItemId]!.Name.Trim());
            return ValueTask.CompletedTask;
        }
        var boxName = items[stack.ItemId]!.Name.Trim();
        var slot = stack.Slot;
        stack.Count--;
        if (stack.Count == 0)
            c.Bag.Remove(stack);
        session.Send(ItemMessages.BagSlotDecrease(slot, 1));
        if (key is not null)
        {
            key.Count--;
            if (key.Count == 0)
                c.Bag.Remove(key);
            session.Send(ItemMessages.BagSlotDecrease(key.Slot, 1));
        }
        var added = Bag.Add(c, items, reward.ItemId, reward.Count).Sum(x => x.Added);
        session.Send(GroundMessages.ItemObtained(reward.ItemId, (ushort)added));
        logger.LogInformation("[{Session}] {Name} opens {Box}: {Item} x{Count}", session.Id, c.Name, boxName, items[reward.ItemId]!.Name.Trim(), added);
        return ValueTask.CompletedTask;
    }
}
