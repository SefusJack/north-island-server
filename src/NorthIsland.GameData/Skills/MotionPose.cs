namespace NorthIsland.GameData.Skills;

/// <summary>One pose of a motion: how long the fighter stays on a path point and how fast it moves on.</summary>
/// <param name="Direction">The facing of the pose.</param>
/// <param name="HoldFrame">The animation frame shown while holding.</param>
/// <param name="Speed">The walking and animation speed.</param>
/// <param name="HoldMs">The time in ms to stay on the pose's point.</param>
public readonly record struct MotionPose(
    byte Direction,
    byte HoldFrame,
    double Speed,
    int HoldMs);
