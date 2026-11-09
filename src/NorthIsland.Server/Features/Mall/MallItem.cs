namespace NorthIsland.Server.Features.Mall;

/// <summary>One configured row of the item mall.</summary>
public sealed class MallItem
{
    /// <summary>The item id sold.</summary>
    public ushort Item { get; set; }

    /// <summary>The mall tab the row appears in.</summary>
    public byte Category { get; set; } = 2;

    /// <summary>How many items one purchase unit gives.</summary>
    public byte Pack { get; set; } = 1;

    /// <summary>Price of one unit in mall points, before the discount.</summary>
    public ushort Price { get; set; }

    /// <summary>Percent of the price that is charged; 100 means no discount.</summary>
    public byte Discount { get; set; } = 100;

    /// <summary>Hot marker sent to the client with the row; 0 for none.</summary>
    public byte Hot { get; set; }

    /// <summary>Rotation group the row belongs to; 0 means it is always shown.</summary>
    public byte Rotation { get; set; }
}
