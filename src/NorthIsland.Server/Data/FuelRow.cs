namespace NorthIsland.Server.Data;

/// <summary>One row of a character's fuel bank.</summary>
public sealed class FuelRow
{
    /// <summary>The row number.</summary>
    public byte Row { get; set; }

    /// <summary>The item's id in Item.dat.</summary>
    public ushort ItemId { get; set; }

    /// <summary>How many items the row holds.</summary>
    public int Count { get; set; }
}
