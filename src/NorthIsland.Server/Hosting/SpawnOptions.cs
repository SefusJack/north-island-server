namespace NorthIsland.Server.Hosting;

/// <summary>The place where a new character starts.</summary>
public sealed class SpawnOptions
{
    /// <summary>The map id.</summary>
    public ushort MapId { get; set; } = 10017; // Deck

    /// <summary>The x position on the map.</summary>
    public ushort X { get; set; } = 1042;

    /// <summary>The y position on the map.</summary>
    public ushort Y { get; set; } = 1075;
}
