namespace NorthIsland.Server.Data;

/// <summary>One cell of a portable box grid (the picnic box or the experience bag).</summary>
public sealed class BoxSlot
{
    /// <summary>The cell number in the grid.</summary>
    public byte Cell { get; set; }

    /// <summary>The item's id in Item.dat.</summary>
    public ushort ItemId { get; set; }

    /// <summary>How many items the cell holds.</summary>
    public byte Count { get; set; }
}
