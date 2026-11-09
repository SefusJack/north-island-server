namespace NorthIsland.Protocol.Messages.World;

/// <summary>One line of the level ranking.</summary>
/// <param name="Job">Job byte of the player.</param>
/// <param name="Level">Level of the player.</param>
/// <param name="Name">Name of the player.</param>
public sealed record LevelRankRow(
    byte Job,
    byte Level,
    string Name);
