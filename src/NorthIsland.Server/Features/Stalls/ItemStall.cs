namespace NorthIsland.Server.Features.Stalls;

/// <summary>A stall of ordinary bag items opened by a player.</summary>
public sealed class ItemStall
{
    /// <summary>The stall's name.</summary>
    public required string Name { get; init; }

    /// <summary>The sign style byte the player chose.</summary>
    public byte Sign { get; init; }

    /// <summary>The price of each offered bag stack, by bag slot.</summary>
    public Dictionary<byte, int> Prices { get; } = [];
}
