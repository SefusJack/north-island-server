namespace NorthIsland.GameData.Skills;

/// <summary>One damage-number hit of a motion.</summary>
/// <param name="At">When the hit's number appears.</param>
/// <param name="Percent">The share of the HP or SP amount this hit shows.</param>
public readonly record struct MotionHit(
    int At,
    byte Percent);
