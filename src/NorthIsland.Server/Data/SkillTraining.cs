using NorthIsland.GameData.Skills;

namespace NorthIsland.Server.Data;

/// <summary>The outcome of training a skill.</summary>
/// <param name="Exp">The skill's experience after training.</param>
/// <param name="Level">The skill's level after training.</param>
/// <param name="LevelRose">True when the level went up.</param>
/// <param name="Successor">The skill learned because the old one reached its maximum level, or null.</param>
public sealed record SkillTraining(
    int Exp,
    byte Level,
    bool LevelRose,
    SkillRecord? Successor);
