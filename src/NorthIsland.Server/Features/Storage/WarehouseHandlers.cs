using Microsoft.Extensions.Logging;
using NorthIsland.Protocol.Messages.Items;

namespace NorthIsland.Server.Features.Storage;

/// <summary>Serves the city warehouse: 50 plain slots of items stored per character.</summary>
/// <param name="items">The item records, for stackability.</param>
/// <param name="logger">The logger.</param>
public sealed class WarehouseHandlers(
    ItemData items,
    ILogger<WarehouseHandlers> logger)
{
    /// <summary>The number of warehouse slots (50).</summary>
    public const int Slots = 50;

    /// <summary>The item flag (32) that keeps an item out of the warehouse.</summary>
    public const ushort NoWarehouseFlag = 32;

    /// <summary>Opens the warehouse: clears every slot on the client, lists the stored stacks and shows the window.</summary>
    /// <param name="session">The player's session.</param>
    public void Open(GameSession session)
    {
        var character = session.ActiveCharacter!;
        for (var slot = 1; slot <= Slots; slot++)
            session.Send(WarehouseMessages.SlotClear((byte)slot));
        if (character.Warehouse.Count > 0)
            session.Send(WarehouseMessages.List(character.Warehouse.OrderBy(w => w.Slot).Select(Entry)));
        session.Send(WarehouseMessages.Open());
        logger.LogInformation("[{Session}] {Name}: warehouse ({Count} stack(s))", session.Id, character.Name, character.Warehouse.Count);
    }

    /// <summary>
    /// Adds items to a warehouse list: a stackable item first onto the first same-item stack under 50, otherwise into the first empty slot. Stops
    /// when no slot is free.
    /// </summary>
    /// <param name="warehouse">The stacks of the warehouse to change.</param>
    /// <param name="items">The item records.</param>
    /// <param name="itemId">The item to add.</param>
    /// <param name="count">How many to add.</param>
    /// <param name="wear">The wear of the item.</param>
    /// <param name="insert">The inserted item.</param>
    /// <param name="fusion">The fusion level.</param>
    /// <param name="pose">The pose item.</param>
    /// <param name="boxItem">The boxed item.</param>
    /// <returns>The stacks that were created or grown.</returns>
    public static List<BagItem> Place(
        List<BagItem> warehouse,
        ItemData items,
        ushort itemId,
        int count,
        byte wear,
        ushort insert,
        byte fusion = 0,
        ushort pose = 0,
        ushort boxItem = 0)
    {
        var touched = new List<BagItem>();
        var stackable = Bag.IsStackable(items[itemId]);
        while (count > 0)
        {
            var stack = stackable ? warehouse.Where(w => w.ItemId == itemId && w.Count < Bag.MaxStack).OrderBy(w => w.Slot).FirstOrDefault() : null;
            if (stack is null)
            {
                var slot = Enumerable.Range(1, Slots).FirstOrDefault(s => !warehouse.Exists(w => w.Slot == s));
                if (slot == 0)
                    break;
                stack = new BagItem
                {
                    Slot = (byte)slot,
                    ItemId = itemId,
                    Count = 0,
                    Wear = wear,
                    Insert = insert,
                    Fusion = fusion,
                    Pose = pose,
                    BoxItem = boxItem
                };
                warehouse.Add(stack);
            }
            var added = Math.Min(count, stackable ? Bag.MaxStack - stack.Count : 1);
            stack.Count += (byte)added;
            count -= added;
            if (!touched.Contains(stack))
                touched.Add(stack);
        }
        return touched;
    }

    /// <summary>
    /// Moves whole bag stacks into the warehouse (<c>23/34</c>). A slot with nothing, with the ridden vehicle or with a no-warehouse item is skipped;
    /// a stack that does not fit ends the order with the full notice.
    /// </summary>
    /// <param name="session">The player's session.</param>
    /// <param name="body">The packet body: one byte per bag slot.</param>
    internal ValueTask OnDeposit(GameSession session, ReadOnlyMemory<byte> body)
    {
        if (session.State != SessionState.InWorld || session.ActiveCharacter is not { } character)
            return ValueTask.CompletedTask;
        var moved = new List<string>();
        var touched = new HashSet<BagItem>();
        foreach (var slot in body.ToArray().Distinct())
        {
            var stack = character.Bag.Find(b => b.Slot == slot);
            if (stack is null || session.Vehicle?.BagSlot == slot || ((items[stack.ItemId]?.Flags ?? 0) & NoWarehouseFlag) != 0)
            {
                logger.LogWarning("[{Session}] warehouse deposit: bag slot {Slot} cannot be stored", session.Id, slot);
                continue;
            }
            var trial = character.Warehouse.Select(Copy).ToList();
            var before = trial.Sum(w => w.ItemId == stack.ItemId ? w.Count : 0);
            Place(trial, items, stack.ItemId, stack.Count, stack.Wear, stack.Insert, stack.Fusion, stack.Pose, stack.BoxItem);
            if (trial.Sum(w => w.ItemId == stack.ItemId ? w.Count : 0) - before != stack.Count)
            {
                session.Send(BankMessages.Full());
                break;
            }
            foreach (var w in Place(character.Warehouse, items, stack.ItemId, stack.Count, stack.Wear, stack.Insert, stack.Fusion, stack.Pose, stack.BoxItem))
                touched.Add(w);
            session.Send(ItemMessages.BagSlotDecrease(stack.Slot, stack.Count));
            character.Bag.Remove(stack);
            moved.Add($"{items[stack.ItemId]?.Name} x{stack.Count}");
        }
        SendSlots(session, touched.Select(t => t.Slot), character);
        logger.LogInformation(
            "[{Session}] {Name} puts in the warehouse: {Items}",
            session.Id,
            character.Name,
            moved.Count > 0 ? string.Join(", ", moved) : "nothing");
        return ValueTask.CompletedTask;
    }

    /// <summary>
    /// Moves whole warehouse stacks into the bag (<c>23/35</c>). An empty slot is skipped; a stack that does not fit ends the order with the bag-full
    /// notice.
    /// </summary>
    /// <param name="session">The player's session.</param>
    /// <param name="body">The packet body: one byte per warehouse slot.</param>
    internal ValueTask OnWithdraw(GameSession session, ReadOnlyMemory<byte> body)
    {
        if (session.State != SessionState.InWorld || session.ActiveCharacter is not { } character)
            return ValueTask.CompletedTask;
        var moved = new List<string>();
        var emptied = new List<byte>();
        foreach (var slot in body.ToArray().Distinct())
        {
            var stack = character.Warehouse.Find(w => w.Slot == slot);
            if (stack is null)
            {
                logger.LogWarning("[{Session}] warehouse withdraw: slot {Slot} is empty", session.Id, slot);
                continue;
            }
            List<BagItem> trial = [.. character.Bag.Select(Copy)];
            if (Bag.Add(trial, items, stack.ItemId, stack.Count).Sum(c => c.Added) != stack.Count)
            {
                session.Send(ShopMessages.BuyResult(BuyResult.InventoryFull));
                break;
            }
            foreach (var (bagStack, added) in Bag.Add(
                character.Bag,
                items,
                stack.ItemId,
                stack.Count,
                stack.Wear,
                stack.Insert,
                stack.Fusion,
                stack.Pose,
                stack.BoxItem))
                session.Send(
                    ItemMessages.BagAdd(
                        new BagEntry
                        {
                            Slot = bagStack.Slot,
                            ItemId = stack.ItemId,
                            Count = added,
                            Wear = stack.Wear,
                            Insert = stack.Insert,
                            FusionLevel = stack.Fusion,
                            PoseItemId = stack.Pose,
                            BoxItemId = stack.BoxItem
                        }));
            character.Warehouse.Remove(stack);
            emptied.Add(stack.Slot);
            moved.Add($"{items[stack.ItemId]?.Name} x{stack.Count}");
        }
        SendSlots(session, emptied, character);
        logger.LogInformation(
            "[{Session}] {Name} takes from the warehouse: {Items}",
            session.Id,
            character.Name,
            moved.Count > 0 ? string.Join(", ", moved) : "nothing");
        return ValueTask.CompletedTask;
    }

    /// <summary>Refreshes warehouse slots on the client: clears each slot, then re-sends those still filled.</summary>
    /// <param name="session">The player's session.</param>
    /// <param name="slots">The slots that changed.</param>
    /// <param name="character">The player's active character.</param>
    private static void SendSlots(GameSession session, IEnumerable<byte> slots, Character character)
    {
        List<WareEntry> filled = [];
        foreach (var slot in slots.Distinct().Order())
        {
            session.Send(WarehouseMessages.SlotClear(slot));
            if (character.Warehouse.Find(w => w.Slot == slot) is { } stack)
                filled.Add(Entry(stack));
        }
        if (filled.Count > 0)
            session.Send(WarehouseMessages.List(filled));
    }

    /// <summary>Converts a stored stack to the entry of a warehouse packet.</summary>
    /// <param name="b">The stored stack.</param>
    /// <returns>The packet entry.</returns>
    private static WareEntry Entry(BagItem b) => new(b.Slot, b.ItemId, b.Count, b.Wear);

    /// <summary>Copies the slot, item, count, wear and insert of a stack, for trying a placement.</summary>
    /// <param name="b">The stack to copy.</param>
    /// <returns>The copy.</returns>
    private static BagItem Copy(BagItem b) => new() { Slot = b.Slot, ItemId = b.ItemId, Count = b.Count, Wear = b.Wear, Insert = b.Insert };
}
