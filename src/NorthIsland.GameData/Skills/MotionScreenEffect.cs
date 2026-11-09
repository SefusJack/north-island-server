namespace NorthIsland.GameData.Skills;

/// <summary>A screen effect that is active between two moments of a motion.</summary>
/// <param name="Kind">The effect: 1 shake, 2 fade, 3 after-images.</param>
/// <param name="From">Start of the window, a pose index 1..100 or ms since the action began.</param>
/// <param name="To">End of the window, a pose index 1..100 or ms since the action began.</param>
public readonly record struct MotionScreenEffect(
    byte Kind,
    int From,
    int To);
