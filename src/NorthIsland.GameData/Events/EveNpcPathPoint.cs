namespace NorthIsland.GameData.Events;

/// <summary>One point of an NPC movement path.</summary>
/// <param name="X">The x position.</param>
/// <param name="Y">The y position.</param>
/// <param name="Value">The third value stored with the point.</param>
public readonly record struct EveNpcPathPoint(
    int X,
    int Y,
    int Value);
