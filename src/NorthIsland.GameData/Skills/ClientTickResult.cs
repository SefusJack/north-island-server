namespace NorthIsland.GameData.Skills;

/// <summary>The times in ms found by one run of <see cref="ClientTickModel"/>.</summary>
/// <param name="PathEndMs">When the attacker has walked its last point and finished its poses.</param>
/// <param name="HitMs">When the targets are hit.</param>
/// <param name="ReactionPathEndMs">When the targets' reaction path ends; 0 when no target plays one.</param>
/// <param name="IndexMs">When each pose index is reached, indexed by the pose index.</param>
public readonly record struct ClientTickResult(
    double PathEndMs,
    double HitMs,
    double ReactionPathEndMs,
    double[] IndexMs);
