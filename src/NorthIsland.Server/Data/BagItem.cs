namespace NorthIsland.Server.Data;

/// <summary>One stack in a bag-like grid: the bag, the bank, the warehouse, the storeroom or the Item Bag.</summary>
public sealed class BagItem
{
    /// <summary>The number of the first bag slot (slots are 1-based).</summary>
    public const int FirstSlot = 1;

    /// <summary>The number of the last bag slot.</summary>
    public const int LastSlot = 50;

    /// <summary>The slot the stack sits in.</summary>
    public byte Slot { get; set; }

    /// <summary>The item's id in Item.dat.</summary>
    public ushort ItemId { get; set; }

    /// <summary>How many items the stack holds.</summary>
    public byte Count { get; set; } = 1;

    /// <summary>The item's wear value, sent to the client with the item.</summary>
    public byte Wear { get; set; } = 1;

    /// <summary>The id of the magic crystal stone set in the item, 0 for none.</summary>
    public ushort Insert { get; set; }

    /// <summary>The fusion level of the item.</summary>
    public byte Fusion { get; set; }

    /// <summary>The item id of the pose look applied to the item, 0 for none.</summary>
    public ushort Pose { get; set; }

    /// <summary>The item's used amount, sent with the item in trades and in the home shop.</summary>
    public int Used { get; set; }

    /// <summary>An extra value sent with the item in trades and in the home shop.</summary>
    public ushort Extra { get; set; }

    /// <summary>The item id of the box the item came in, 0 for none.</summary>
    public ushort BoxItem { get; set; }
}
