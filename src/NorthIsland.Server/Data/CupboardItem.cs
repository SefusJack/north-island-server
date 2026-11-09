namespace NorthIsland.Server.Data;

/// <summary>A stack stored in a cupboard of the house.</summary>
public sealed class CupboardItem
{
    /// <summary>The slot in the cupboard.</summary>
    public byte Slot { get; set; }

    /// <summary>The number the client names the stored stack by when it takes it out.</summary>
    public byte Id { get; set; }

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

    /// <summary>The item id of the box the item came in, 0 for none.</summary>
    public ushort BoxItem { get; set; }
}
