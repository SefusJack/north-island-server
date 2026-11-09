namespace NorthIsland.Server.Features.Activities;

/// <summary>One prize of the online draw.</summary>
public sealed class OnlineDrawPrize
{
    /// <summary>The item id of the prize.</summary>
    public ushort Item { get; set; }

    /// <summary>How many of the item the prize gives.</summary>
    public byte Count { get; set; } = 1;

    /// <summary>The relative chance of the prize.</summary>
    public int Weight { get; set; } = 1;

    /// <summary>True when a win is announced to the 2014 clients online.</summary>
    public bool Top { get; set; }
}
