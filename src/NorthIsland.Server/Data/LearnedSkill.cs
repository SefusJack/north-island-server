namespace NorthIsland.Server.Data;

/// <summary>A skill a character knows, with its level and experience.</summary>
public sealed class LearnedSkill
{
    /// <summary>Skill id.</summary>
    public ushort Id { get; set; }

    /// <summary>Current skill level.</summary>
    public byte Level { get; set; } = 1;

    /// <summary>Skill experience gathered.</summary>
    public int Exp { get; set; }
}
