namespace NorthIsland.Server.Data;

/// <summary>An item put into a box-type furniture piece of the house.</summary>
public sealed class BoxItem
{
    /// <summary>The kind of box furniture, 1 to 5.</summary>
    public byte Kind { get; set; }

    /// <summary>The slot inside the box.</summary>
    public byte Slot { get; set; }

    /// <summary>The item's id in Item.dat.</summary>
    public ushort ItemId { get; set; }

    /// <summary>How many items the stack holds.</summary>
    public byte Count { get; set; } = 1;
}
