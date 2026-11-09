namespace NorthIsland.GameData.Events;

/// <summary>A convey (warp) record: where a warp out of the map leads.</summary>
public sealed record EveConvey
{
    /// <summary>The convey index within the scene.</summary>
    public int Index { get; init; }

    /// <summary>The convey name.</summary>
    public string Name { get; init; } = "";

    /// <summary>The map the warp leads to.</summary>
    public ushort TargetMapId { get; init; }

    /// <summary>The target x position.</summary>
    public int X { get; init; }

    /// <summary>The target y position.</summary>
    public int Y { get; init; }

    /// <summary>The facing after arriving.</summary>
    public byte Direction { get; init; }

    /// <summary>Word read after the direction (named after its offset 6); its meaning is not established.</summary>
    public ushort Unknown6 { get; init; }
}
