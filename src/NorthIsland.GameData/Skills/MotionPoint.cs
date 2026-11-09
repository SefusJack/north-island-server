namespace NorthIsland.GameData.Skills;

/// <summary>A point on the battle screen.</summary>
/// <param name="X">Horizontal position in pixels.</param>
/// <param name="Y">Vertical position in pixels.</param>
public readonly record struct MotionPoint(
    int X,
    int Y);
