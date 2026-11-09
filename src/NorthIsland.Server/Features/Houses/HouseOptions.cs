namespace NorthIsland.Server.Features.Houses;

/// <summary>The settings of the houses feature.</summary>
public sealed class HouseOptions
{
    /// <summary>The numbers for life in the house.</summary>
    public HouseLifeOptions HouseLife { get; set; } = new();
}
