using NorthIsland.GameData.Skills;

namespace NorthIsland.Server.Features.Battles;

/// <summary>The rules for combination attacks, several attackers of one side hitting one target together.</summary>
public static class ComboRules
{
    /// <summary>Tells whether an attack may be part of a combination.</summary>
    /// <param name="skill">The skill used, or null for a plain hand attack.</param>
    /// <returns>True for a hand attack and for a damage skill aimed at one enemy.</returns>
    public static bool IsSingleTargetAttack(SkillRecord? skill) => skill is null or { Type: 1 or 2, TargetType: 0 };

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
