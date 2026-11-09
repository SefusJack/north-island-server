using NorthIsland.GameData.Items;

namespace NorthIsland.Server.World;

/// <summary>The 50-slot bag grid (5 columns by 10 rows): static helpers that count, fit, add and remove stacks in a list of stacks.</summary>
public static class Bag
{
    /// <summary>The number of columns of the grid.</summary>
    public const int Columns = 5;

    /// <summary>The number of rows of the bag grid.</summary>
    public const int Rows = 10;

    /// <summary>The most items one stack holds.</summary>
    public const byte MaxStack = 50;

    /// <summary>Whether items of this kind share a stack: plain items that are not equipment, vehicles, large, timed or bottles.</summary>
    /// <param name="r">The item's Item.dat record, null for an unknown item.</param>
    /// <returns>True when the item stacks; false for an unknown item.</returns>
    public static bool IsStackable(ItemRecord? r) =>
        r is not null && !r.IsEquipment && r.EquipSlot is not (7 or 10) && r.CellWidth <= 1 && r.CellHeight <= 1
            && r.UsePeriod == 0 && r.SubType != 130 && Bottles.KindOf(r) == 0;

    /// <summary>Adds up the counts of all stacks of an item in a grid.</summary>
    /// <param name="bag">The bag-like grid (a list of stacks).</param>
    /// <param name="itemId">The item id.</param>
    /// <returns>The total number of items.</returns>
    public static int Count(List<BagItem> bag, ushort itemId) =>
        bag.Where(b => b.ItemId == itemId).Sum(b => b.Count);

    /// <summary>Whether an item fits with its top-left cell at a slot: inside the grid and not overlapping another stack.</summary>
    /// <param name="bag">The bag-like grid (a list of stacks).</param>
    /// <param name="items">Item.dat, for the item sizes.</param>
    /// <param name="itemId">The item id.</param>
    /// <param name="slot">The slot of the item's top-left cell.</param>
    /// <param name="ignore">A stack left out of the occupied cells, such as the one being moved, or null.</param>
    /// <param name="rows">The number of rows of the grid.</param>
    /// <returns>True when the item fits.</returns>
    public static bool Fits(
        List<BagItem> bag,
        ItemData items,
        ushort itemId,
        byte slot,
        BagItem? ignore = null,
        int rows = Rows)
    {
        var cells = CellsOf(slot, Size(items, itemId), rows);
        if (cells is null)
            return false;
        var occupied = Occupied(bag, items, ignore);
        return cells.TrueForAll(c => !occupied[c]);
    }

    /// <summary>Finds the stack that covers a cell, counting every cell of a large item.</summary>
    /// <param name="bag">The bag-like grid (a list of stacks).</param>
    /// <param name="items">Item.dat, for the item sizes.</param>
    /// <param name="slot">The cell number.</param>
    /// <returns>The stack, or null when the cell is empty.</returns>
    public static BagItem? Covering(List<BagItem> bag, ItemData items, int slot) =>
        bag.Find(b => (CellsOf(b.Slot, Size(items, b.ItemId)) ?? [b.Slot]).Contains(slot));

    /// <summary>Finds the first slot where an item of this size fits.</summary>
    /// <param name="bag">The bag-like grid (a list of stacks).</param>
    /// <param name="items">Item.dat, for the item sizes.</param>
    /// <param name="itemId">The item id.</param>
    /// <returns>The slot, or 0 when none fits.</returns>
    public static byte FreeSlot(List<BagItem> bag, ItemData items, ushort itemId)
    {
        var size = Size(items, itemId);
        var occupied = Occupied(bag, items, null);
        for (var slot = (byte)BagItem.FirstSlot; slot <= BagItem.LastSlot; slot++)
        {
            if (CellsOf(slot, size) is { } cells && cells.TrueForAll(c => !occupied[c]))
                return slot;
        }
        return 0;
    }

    /// <summary>
    /// Puts items into a grid: first a non-full stack of the same item when it stacks, then new stacks in free slots, until
    /// all are placed or the grid is full.
    /// </summary>
    /// <param name="bag">The bag-like grid (a list of stacks).</param>
    /// <param name="items">Item.dat, for the item sizes.</param>
    /// <param name="itemId">The item id.</param>
    /// <param name="count">How many items to add.</param>
    /// <param name="wear">The wear value of new stacks.</param>
    /// <param name="insert">The id of the magic crystal stone set into new stacks, 0 for none.</param>
    /// <param name="fusion">The fusion level of new stacks.</param>
    /// <param name="pose">The item id of the pose look of new stacks, 0 for none.</param>
    /// <param name="boxItem">The item id of the box the item came in, 0 for none.</param>
    /// <returns>One entry per stack touched, with the number added to it; the numbers add up to less than count when the bag is full.</returns>
    public static List<(BagItem Item, byte Added)> Add(
        List<BagItem> bag,
        ItemData items,
        ushort itemId,
        int count,
        byte wear = 1,
        ushort insert = 0,
        byte fusion = 0,
        ushort pose = 0,
        ushort boxItem = 0)
    {
        var changes = new List<(BagItem, byte)>();
        var stackable = IsStackable(items[itemId]);
        while (count > 0)
        {
            var stack = stackable
                ? bag.Where(b => b.ItemId == itemId && b.Count < MaxStack).OrderBy(b => b.Slot).FirstOrDefault()
                : null;
            if (stack is null)
            {
                var slot = FreeSlot(bag, items, itemId);
                if (slot == 0)
                    break;
                stack = new BagItem { Slot = slot, ItemId = itemId, Count = 0, Wear = wear, Insert = insert, Fusion = fusion, Pose = pose, BoxItem = boxItem };
                bag.Add(stack);
            }
            var added = (byte)Math.Min(count, stackable ? MaxStack - stack.Count : 1);
            stack.Count += added;
            count -= added;
            changes.Add((stack, added));
        }
        return changes;
    }

    /// <summary>Takes items out of the stacks of an item, lowest slots first, and drops stacks that become empty.</summary>
    /// <param name="bag">The bag-like grid (a list of stacks).</param>
    /// <param name="itemId">The item id.</param>
    /// <param name="count">How many items to remove.</param>
    /// <returns>The number actually removed, at most count.</returns>
    public static int Remove(List<BagItem> bag, ushort itemId, int count)
    {
        var removed = 0;
        foreach (var stack in bag.Where(b => b.ItemId == itemId).OrderBy(b => b.Slot).ToList())
        {
            if (removed == count)
                break;
            var take = Math.Min(count - removed, stack.Count);
            stack.Count -= (byte)take;
            removed += take;
            if (stack.Count == 0)
                bag.Remove(stack);
        }
        return removed;
    }

    /// <summary>The cells a stack covers: every cell of its rectangle for a large item, else its own slot.</summary>
    /// <param name="items">Item.dat, for the item sizes.</param>
    /// <param name="stack">The stack.</param>
    /// <param name="rows">The number of rows of the grid.</param>
    /// <returns>The cell numbers.</returns>
    public static List<int> CellsOfStack(ItemData items, BagItem stack, int rows = Rows) =>
        CellsOf(stack.Slot, Size(items, stack.ItemId), rows) ?? [stack.Slot];

    /// <summary>The width and height of an item in cells; 1 by 1 for an unknown item or one without a size.</summary>
    /// <param name="items">Item.dat, for the item sizes.</param>
    /// <param name="itemId">The item id.</param>
    /// <returns>The size as (Width, Height).</returns>
    private static (int Width, int Height) Size(ItemData items, ushort itemId)
    {
        var r = items[itemId];
        return r is null || r.CellWidth == 0 || r.CellHeight == 0 ? (1, 1) : (r.CellWidth, r.CellHeight);
    }

    /// <summary>The cell numbers of a rectangle of the given size whose top-left cell is a slot.</summary>
    /// <param name="slot">The slot of the top-left cell.</param>
    /// <param name="size">The width and height in cells.</param>
    /// <param name="rows">The number of rows of the grid.</param>
    /// <returns>The cells, or null when the slot is out of range or the rectangle leaves the grid.</returns>
    private static List<int>? CellsOf(int slot, (int Width, int Height) size, int rows = Rows)
    {
        if (slot is < BagItem.FirstSlot or > BagItem.LastSlot)
            return null;
        var row = (slot - 1) / Columns;
        var column = slot - Columns * row;
        if (column + size.Width - 1 > Columns || row + size.Height - 1 > rows - 1)
            return null;
        var cells = new List<int>(size.Width * size.Height);
        for (var y = row; y < row + size.Height; y++)
            for (var x = column; x < column + size.Width; x++)
                cells.Add(Columns * y + x);
        return cells;
    }

    /// <summary>Marks the cells covered by the stacks of a grid.</summary>
    /// <param name="bag">The bag-like grid (a list of stacks).</param>
    /// <param name="items">Item.dat, for the item sizes.</param>
    /// <param name="ignore">A stack to leave out, or null.</param>
    /// <returns>A flag per cell number, true when a stack covers the cell.</returns>
    private static bool[] Occupied(List<BagItem> bag, ItemData items, BagItem? ignore)
    {
        var occupied = new bool[BagItem.LastSlot + 1];
        foreach (var b in bag)
        {
            if (ReferenceEquals(b, ignore))
                continue;
            foreach (var c in CellsOf(b.Slot, Size(items, b.ItemId)) ?? [b.Slot])
                occupied[c] = true;
        }
        return occupied;
    }

    /// <summary>Adds up the counts of an item in the character's bag.</summary>
    /// <param name="character">The character whose bag is used.</param>
    /// <param name="itemId">The item id.</param>
    /// <returns>The total number of items.</returns>
    public static int Count(Character character, ushort itemId) => Count(character.Bag, itemId);

    /// <summary>Whether an item fits with its top-left cell at a slot of the character's bag.</summary>
    /// <param name="character">The character whose bag is used.</param>
    /// <param name="items">Item.dat, for the item sizes.</param>
    /// <param name="itemId">The item id.</param>
    /// <param name="slot">The slot of the item's top-left cell.</param>
    /// <param name="ignore">A stack left out of the occupied cells, or null.</param>
    /// <returns>True when the item fits.</returns>
    public static bool Fits(
        Character character,
        ItemData items,
        ushort itemId,
        byte slot,
        BagItem? ignore = null) =>
            Fits(character.Bag, items, itemId, slot, ignore);

    /// <summary>Finds the first slot of the character's bag where an item fits.</summary>
    /// <param name="character">The character whose bag is used.</param>
    /// <param name="items">Item.dat, for the item sizes.</param>
    /// <param name="itemId">The item id.</param>
    /// <returns>The slot, or 0 when none fits.</returns>
    public static byte FreeSlot(Character character, ItemData items, ushort itemId) => FreeSlot(character.Bag, items, itemId);

    /// <summary>Puts items into the character's bag, as the overload for a list of stacks does.</summary>
    /// <param name="character">The character whose bag is used.</param>
    /// <param name="items">Item.dat, for the item sizes.</param>
    /// <param name="itemId">The item id.</param>
    /// <param name="count">How many items to add.</param>
    /// <param name="wear">The wear value of new stacks.</param>
    /// <param name="insert">The id of the magic crystal stone set into new stacks, 0 for none.</param>
    /// <param name="fusion">The fusion level of new stacks.</param>
    /// <param name="pose">The item id of the pose look of new stacks, 0 for none.</param>
    /// <param name="boxItem">The item id of the box the item came in, 0 for none.</param>
    /// <returns>One entry per stack touched, with the number added to it; the numbers add up to less than count when the bag is full.</returns>
    public static List<(BagItem Item, byte Added)> Add(
        Character character,
        ItemData items,
        ushort itemId,
        int count,
        byte wear = 1,
        ushort insert = 0,
        byte fusion = 0,
        ushort pose = 0,
        ushort boxItem = 0) =>
            Add(character.Bag, items, itemId, count, wear, insert, fusion, pose, boxItem);

    /// <summary>Takes items out of the character's bag, lowest slots first.</summary>
    /// <param name="character">The character whose bag is used.</param>
    /// <param name="itemId">The item id.</param>
    /// <param name="count">How many items to remove.</param>
    /// <returns>The number actually removed, at most count.</returns>
    public static int Remove(Character character, ushort itemId, int count) => Remove(character.Bag, itemId, count);

    /// <summary>Splits stacks of more than one bottle in the character's bag, bank and warehouse into one-item stacks.</summary>
    /// <param name="character">The character whose bag is used.</param>
    /// <param name="items">Item.dat, for the item sizes.</param>
    /// <returns>The number of single items split off.</returns>
    public static int SplitLegacyStacks(Character character, ItemData items) =>
        SplitLegacyStacks(character.Bag, items) + SplitLegacyStacks(character.Bank, items) + SplitLegacyStacks(character.Warehouse, items);

    /// <summary>Splits stacks of more than one bottle in one grid into one-item stacks in free slots, as far as there is room.</summary>
    /// <param name="list">The grid to split in.</param>
    /// <param name="items">Item.dat, for the item sizes.</param>
    /// <returns>The number of single items split off.</returns>
    public static int SplitLegacyStacks(List<BagItem> list, ItemData items)
    {
        var split = 0;
        foreach (var stack in list.Where(b => b.Count > 1 && items[b.ItemId] is { } r && Bottles.KindOf(r) != 0).ToList())
            while (stack.Count > 1)
            {
                var slot = FreeSlot(list, items, stack.ItemId);
                if (slot == 0)
                    break;
                stack.Count--;
                list.Add(
                    new BagItem
                    {
                        Slot = slot,
                        ItemId = stack.ItemId,
                        Count = 1,
                        Wear = stack.Wear,
                        Insert = stack.Insert,
                        Fusion = stack.Fusion,
                        Pose = stack.Pose,
                        BoxItem = stack.BoxItem,
                        Used = stack.Used
                    });
                split++;
            }
        return split;
    }
}