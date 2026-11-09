namespace NorthIsland.Server.Features.Mall;

/// <summary>Options of the mall feature.</summary>
public sealed class MallOptions
{
    /// <summary>The item mall settings and catalog.</summary>
    public ItemMallOptions ItemMall { get; set; } = new();

    /// <summary>The settings of the mall boxes.</summary>
    public MallBoxOptions MallBoxes { get; set; } = new();

    /// <summary>Gold an exchanged point is worth in the point exchange form of the ATM.</summary>
    public int PointExchangeRate { get; set; } = 1000;
}
