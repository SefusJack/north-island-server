namespace NorthIsland.Server.Features.Progression;

/// <summary>Settings of the progression feature.</summary>
public sealed class ProgressionOptions
{
    /// <summary>Stat points a character gets for every level gained.</summary>
    public int StatPointsPerLevel { get; set; } = 3;

    /// <summary>Skill points a character gets for every level gained.</summary>
    public int SkillPointsPerLevel { get; set; } = 1;
}
