using NorthIsland.GameData.Skills;

namespace NorthIsland.Server.Features.Battles;

/// <summary>The rules for combination attacks, several attackers of one side hitting one target together.</summary>
public static class ComboRules
{
    /// <summary>Tells whether an attack may be part of a combination.</summary>
    /// <param name="skill">The skill used, or null for a plain hand attack.</param>
    /// <param name="level">The attacker's level of the skill, which picks its target area.</param>
    /// <returns>True for a hand attack and for a damage skill aimed at one enemy whose area at that level is the enemy alone.</returns>
    public static bool IsSingleTargetAttack(SkillRecord? skill, int level) =>
        skill is null || skill is { Type: 1 or 2, TargetType: 0 } && !Battle.IsSpread(skill.AreaAt(level));

    /// <summary>The fighter's level as the combination odds count it, with a bonus per rebirth.</summary>
    /// <param name="f">The fighter.</param>
    /// <param name="o">The battle options.</param>
    /// <returns>The level plus the rebirth count times <see cref="BattleOptions.ComboRebornLevelBonus"/>.</returns>
    public static int EffectiveLevel(Fighter f, BattleOptions o) => f.Level + f.Reborn * o.ComboRebornLevelBonus;

    /// <summary>The average effective level of one side for the combination odds.</summary>
    /// <param name="battle">The battle.</param>
    /// <param name="side">The side whose fighters are averaged.</param>
    /// <param name="o">The battle options; <see cref="BattleOptions.ComboOddsLevels"/> picks who counts.</param>
    /// <param name="members">The fighters in the combination so far plus the one about to join, or null.</param>
    /// <returns>The average, or 0 when nobody counts.</returns>
    public static double AverageLevel(Battle battle, byte side, BattleOptions o, IReadOnlyCollection<Fighter>? members = null)
    {
        var standing = battle.All.Where(f => f.Side == side && f.IsAlive).ToList();
        var counted = o.ComboOddsLevels switch
        {
            ComboOddsLevels.Players when standing.Exists(f => f.Kind == 2) => standing.FindAll(f => f.Kind == 2), // 2 player, 2 player
            ComboOddsLevels.Members when members is { Count: > 0 } => [.. members], // members only: the combination's members
            _ => standing, // any other case: every fighter still standing
        };
        return counted.Count == 0 ? 0 : counted.Average(f => EffectiveLevel(f, o));
    }

    /// <summary>The chance that a combination forms against a target.</summary>
    /// <param name="averageLevel">The average level of the attacking side, see <see cref="AverageLevel"/>.</param>
    /// <param name="targetLevel">The level of the target.</param>
    /// <param name="o">The battle options with the gap thresholds and percentages.</param>
    /// <returns>The chance in percent, 0 to 100.</returns>
    public static double ChancePercent(double averageLevel, int targetLevel, BattleOptions o)
    {
        var gap = averageLevel - targetLevel;
        if (gap >= o.ComboFullLevelGap)
            return 100;
        if (o.ComboChanceCurve == ComboChanceCurve.Sliding)
        {
            var maxMobLevel = averageLevel - o.ComboFullLevelGap;
            var reach = targetLevel <= 0 ? 0 : Math.Clamp(maxMobLevel / targetLevel, 0, 1);
            return o.ComboBasePercent + (100 - o.ComboBasePercent) * reach;
        }
        return gap >= o.ComboHighLevelGap ? o.ComboHighPercent
            : gap >= o.ComboMidLevelGap ? o.ComboMidPercent
                : o.ComboBasePercent;
    }

    /// <summary>Tells whether two fighters' turn speeds are close enough for them to combine.</summary>
    /// <param name="previous">The fighter already in the combination.</param>
    /// <param name="next">The fighter about to join.</param>
    /// <param name="o">The battle options.</param>
    /// <returns>True when the speeds differ by at most <see cref="BattleOptions.ComboSpeedRange"/>.</returns>
    public static bool WithinSpeedRange(Fighter previous, Fighter next, BattleOptions o) =>
        Math.Abs(BattleFormulas.TurnSpeed(previous) - BattleFormulas.TurnSpeed(next)) <= o.ComboSpeedRange;

    /// <summary>Adds the configured combination damage bonus to a damage value.</summary>
    /// <param name="damage">The damage before the bonus.</param>
    /// <param name="o">The battle options.</param>
    /// <returns>The damage raised by <see cref="BattleOptions.ComboDamageBonusPercent"/> percent.</returns>
    public static int WithBonus(int damage, BattleOptions o) =>
        o.ComboDamageBonusPercent == 0 ? damage : (int)Math.Min(int.MaxValue, Math.Round(damage * (100 + o.ComboDamageBonusPercent) / 100.0));

    /// <summary>How long the server waits for the client to play a combination.</summary>
    /// <param name="animationMs">The animation time of each attack in milliseconds.</param>
    /// <param name="marginMs">Extra time added to the longest animation.</param>
    /// <returns>The longest animation plus the margin, at least 500 milliseconds.</returns>
    public static int WaitMs(IEnumerable<int> animationMs, int marginMs) => Math.Max(500, animationMs.DefaultIfEmpty(0).Max() + marginMs);
}
