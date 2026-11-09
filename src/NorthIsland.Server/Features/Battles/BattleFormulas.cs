using System.Collections.Immutable;
using NorthIsland.GameData.Npcs;
using NorthIsland.GameData.Skills;

namespace NorthIsland.Server.Features.Battles;

/// <summary>The damage, experience, capture, flee, drop and turn order formulas of battles.</summary>
public static class BattleFormulas
{
    /// <summary>Rolls a physical hit from strength, level and worn attack against constitution and worn defence.</summary>
    /// <param name="attacker">The attacking fighter.</param>
    /// <param name="target">The fighter hit.</param>
    /// <param name="rng">The random source for the critical hit and the 90 to 110 percent spread.</param>
    /// <param name="power">An amount added to the base damage.</param>
    /// <returns>The damage, at least 1, and whether the hit was a critical hit.</returns>
    public static DamageRoll PhysicalHit(Fighter attacker, Fighter target, Random rng, int power = 0)
    {
        var baseDamage = Math.Max(1, attacker.Strength * 2 + attacker.TotalLevel * 2 + attacker.Attack - target.Constitution - target.Defence + power);
        var critical = attacker.CriticalPercent > 0 && rng.Next(100) < attacker.CriticalPercent;
        return new DamageRoll(
            Scaled(
                baseDamage * (critical ? 2 : 1) * (0.9 + rng.NextDouble() * 0.2) * JobScale(attacker, Career.Killer) / JobScale(target, Career.Warrior),
                attacker,
                target),
            critical);
    }

    /// <summary>Rolls a physical hit and keeps only its damage.</summary>
    /// <param name="attacker">The attacking fighter.</param>
    /// <param name="target">The fighter hit.</param>
    /// <param name="rng">The random source.</param>
    /// <returns>The damage of <see cref="PhysicalHit"/>.</returns>
    public static int PhysicalDamage(Fighter attacker, Fighter target, Random rng) => PhysicalHit(attacker, target, rng).Damage;

    /// <summary>Applies the attacker's and the target's status scales to a damage value.</summary>
    /// <param name="damage">The damage before the scales.</param>
    /// <param name="attacker">The attacking fighter.</param>
    /// <param name="target">The fighter hit.</param>
    /// <returns>The rounded damage, at least 1.</returns>
    private static int Scaled(double damage, Fighter attacker, Fighter target) =>
        Math.Max(1, (int)Math.Round(damage * StatusRules.AttackScale(attacker) * StatusRules.DefenceScale(target)));

    /// <summary>The HP or SP a healing skill restores.</summary>
    /// <param name="caster">The fighter who casts the skill.</param>
    /// <param name="power">The skill's power.</param>
    /// <returns>The amount restored, at least 1.</returns>
    public static int HealAmount(Fighter caster, ushort power) => Math.Max(1, power + caster.Intelligence * 2 + caster.TotalLevel * 2);

    /// <summary>The bonus of a career for a fighter.</summary>
    /// <param name="f">The fighter.</param>
    /// <param name="job">The career id the bonus belongs to (a Seer also counts for the Knight bonus).</param>
    /// <returns>1.1 for a player with that career, otherwise 1.</returns>
    public static double JobScale(Fighter f, byte job) =>
        f.Kind == 2 && (f.Job == job || job == Career.Knight && f.Job == Career.Seer) ? 1.1 : 1.0; // player

    /// <summary>The damage factor of the element wheel.</summary>
    /// <param name="attacker">The attacking element, 1 to 4.</param>
    /// <param name="target">The element of the target, 1 to 4.</param>
    /// <param name="advantage">The factor when the attacker's element overcomes the target's.</param>
    /// <param name="disadvantage">The factor when the target's element overcomes the attacker's.</param>
    /// <returns>The advantage or disadvantage factor, or 1 when neither applies or an element is outside 1 to 4.</returns>
    public static double ElementScale(byte attacker, byte target, double advantage, double disadvantage)
    {
        if (attacker is < 1 or > 4 || target is < 1 or > 4)
            return 1;
        if (target == attacker % 4 + 1)
            return advantage;
        return attacker == target % 4 + 1 ? disadvantage : 1;
    }

    /// <summary>The factor on the damage a target takes while it carries Cuss, Shrink or Freeze.</summary>
    /// <param name="target">The fighter that takes the damage.</param>
    /// <param name="o">The battle options with the percentages.</param>
    /// <returns>The product of the factors of the debuffs on the target, 1 when it has none.</returns>
    public static double DebuffDamageScale(Fighter target, BattleOptions o) =>
        (target.HasEffect(StatusRules.Cuss) ? 1 + Math.Max(0, o.CussDamagePercent) / 100 : 1)
            * (target.HasEffect(StatusRules.Shrink) ? 1 + Math.Max(0, o.ShrinkDamagePercent) / 100 : 1)
            * (target.HasEffect(StatusRules.Freeze) ? 1 + Math.Max(0, o.FreezeDamagePercent) / 100 : 1);

    /// <summary>The damage factor by attacking element (row) and target element (column), indexed by <see cref="ElementSlot"/>.</summary>
    private static readonly ImmutableArray<ImmutableArray<double>> ElementTable =
        [
            [1.0, 1.0, 0.9, 1.0, 1.0],
            [1.3, 1.3, 1.2, 0.7, 1.6],
            [1.0, 1.0, 0.9, 1.3, 0.6],
            [0.9, 1.4, 0.5, 0.9, 0.9],
            [1.0, 0.7, 1.3, 1.0, 1.0],
        ];

    /// <summary>Maps an element id to its row and column in <see cref="ElementTable"/>.</summary>
    /// <param name="element">The element id.</param>
    /// <returns>The table index, 0 for an element without a place of its own.</returns>
    private static int ElementSlot(byte element) => element switch
    {
        3 => 1, // element 3: slot 1
        1 => 2, // element 1: slot 2
        2 => 3, // element 2: slot 3
        4 => 4, // element 4: slot 4
        _ => 0 // any other element: slot 0
    };

    /// <summary>Looks up the damage factor of an element pair in the element table.</summary>
    /// <param name="attack">The element of the attack.</param>
    /// <param name="target">The element of the target.</param>
    /// <returns>The factor from <see cref="ElementTable"/>.</returns>
    public static double WindowElementScale(byte attack, byte target) => ElementTable[ElementSlot(attack)][ElementSlot(target)];

    /// <summary>The factor a combination attack multiplies the damage with, so that the element factor gains 0.3.</summary>
    /// <param name="attack">The element of the attack.</param>
    /// <param name="target">The element of the target.</param>
    /// <returns>The element factor plus 0.3, divided by the element factor.</returns>
    public static double ComboFactor(byte attack, byte target)
    {
        var e = WindowElementScale(attack, target);
        return (e + 0.3) / e;
    }

    /// <summary>The element an attack counts as.</summary>
    /// <param name="attacker">The attacking fighter.</param>
    /// <param name="skill">The skill used, or null for a hand attack.</param>
    /// <returns>The attacker's element for a hand attack or the normal attack, otherwise the element tree of the skill.</returns>
    public static byte AttackElement(Fighter attacker, SkillRecord? skill) =>
        skill is null || SkillTable.IsNormalAttack(skill.Id) ? attacker.Element : skill.Tree;

    /// <summary>The factor on an attack's strength by the number of targets it hits.</summary>
    /// <param name="targets">The number of targets.</param>
    /// <param name="o">The battle options that hold the factors.</param>
    /// <returns>The configured factor for one, two, four or more targets.</returns>
    public static double TargetCountFactor(int targets, BattleOptions o) => targets switch
    {
        <= 1 => o.WindowAttackFactor, // one target or fewer: the base factor
        2 => o.WindowAttackFactor2Targets, // two targets: the two-target factor
        4 => o.WindowAttackFactor4Targets, // four targets: the four-target factor
        _ => o.WindowAttackFactorManyTargets, // any other count: the many-targets factor
    };

    /// <summary>The part of the defence that is subtracted from an attack.</summary>
    /// <param name="attack">The attack value.</param>
    /// <param name="defence">The defence value.</param>
    /// <returns>98 percent of the defence when the attack is the larger, otherwise 98 percent of the attack plus 17.9 percent of the difference.</returns>
    public static double DefenceTerm(double attack, double defence) =>
        attack >= defence ? 0.98 * defence : 0.98 * attack + 0.179 * (defence - attack);

    /// <summary>Rolls the damage of a hit from the fighters' window values (the attack and defence shown in the client's status window).</summary>
    /// <param name="attacker">The attacking fighter.</param>
    /// <param name="target">The fighter hit.</param>
    /// <param name="rng">The random source for the critical hit and the 90 to 110 percent spread.</param>
    /// <param name="skill">The skill used, or null for a hand attack.</param>
    /// <param name="o">The battle options.</param>
    /// <param name="targets">The number of targets the attack hits.</param>
    /// <returns>The damage, at least 1, and whether the hit was a critical hit.</returns>
    public static DamageRoll WindowHit(
        Fighter attacker,
        Fighter target,
        Random rng,
        SkillRecord? skill,
        BattleOptions o,
        int targets = 1)
    {
        var magic = skill is { Type: 2 };
        var tripled = StatusRules.IsHotFire(attacker) ? Math.Max(0, o.HotFireMultiplier) : 1;
        var power = skill is null || SkillTable.IsNormalAttack(skill.Id) ? 0 : skill.Power;
        double attack = (magic ? attacker.WindowMagicAttack : attacker.WindowAttack)
            * (o.HotFireOn == HotFireMode.Attack ? tripled : 1)
            * TargetCountFactor(targets, o)
            + power;
        double defence = magic ? target.WindowMagicDefence : target.WindowDefence;
        var damage = Math.Max(0, attack - DefenceTerm(attack, defence)) * WindowElementScale(AttackElement(attacker, skill), target.Element);
        if (o.HotFireOn == HotFireMode.Damage)
            damage *= tripled;
        var critical = attacker.CriticalPercent > 0 && rng.Next(100) < attacker.CriticalPercent;
        damage *= (critical ? 2 : 1)
            * (0.9 + rng.NextDouble() * 0.2)
            * StatusRules.AttackScale(attacker, hotFireCounted: true)
            * StatusRules.DefenceScale(target)
            * DebuffDamageScale(target, o);
        return new DamageRoll(Math.Max(1, (int)Math.Min(int.MaxValue, Math.Round(damage))), critical);
    }

    /// <summary>
    /// Rolls a hit with <see cref="WindowHit"/> and applies the damage scale, the defend halving, the combination factor and the
    /// damage cap.
    /// </summary>
    /// <param name="attacker">The attacking fighter.</param>
    /// <param name="target">The fighter hit.</param>
    /// <param name="rng">The random source.</param>
    /// <param name="skill">The skill used, or null for a hand attack.</param>
    /// <param name="o">The battle options.</param>
    /// <param name="combo">True when the hit is part of a combination attack.</param>
    /// <param name="damageScale">A factor applied to the damage.</param>
    /// <param name="halved">True when the target defended, which halves the damage.</param>
    /// <returns>The final damage and whether the hit was a critical hit.</returns>
    public static DamageRoll WindowBlow(
        Fighter attacker,
        Fighter target,
        Random rng,
        SkillRecord? skill,
        BattleOptions o,
        bool combo,
        double damageScale = 1,
        bool halved = false)
    {
        var hit = WindowHit(attacker, target, rng, skill, o);
        var damage = Scale(hit.Damage, damageScale);
        if (halved)
            damage = Defended(damage);
        if (combo)
            damage = Scale(damage, ComboFactor(AttackElement(attacker, skill), target.Element));
        return hit with { Damage = o.MaxDamagePerHit > 0 ? Math.Min(damage, o.MaxDamagePerHit) : damage };
    }

    /// <summary>Tells whether an element overcomes another on the element wheel.</summary>
    /// <param name="attacker">The element that would overcome, 1 to 4.</param>
    /// <param name="target">The element that would be overcome, 1 to 4.</param>
    /// <returns>True when <paramref name="target"/> is the element after <paramref name="attacker"/> on the wheel.</returns>
    public static bool Overcomes(byte attacker, byte target) => attacker is >= 1 and <= 4 && target is >= 1 and <= 4 && target == attacker % 4 + 1;

    /// <summary>The element a skill hits as when it differs from its user's element.</summary>
    /// <param name="skill">The skill, or null.</param>
    /// <returns>The element tree of the Fervidity skill, null for any other skill.</returns>
    public static byte? SkillElement(SkillRecord? skill) =>
        skill is { Id: SkillTable.Fervidity, Tree: >= 1 and <= 4 } ? skill.Tree : null;

    /// <summary>Tells whether a skill cannot hit a target because of the target's element.</summary>
    /// <param name="skill">The skill, or null.</param>
    /// <param name="target">The fighter aimed at.</param>
    /// <returns>True when the skill has an element of its own and the target's element overcomes it.</returns>
    public static bool IsVoidAgainst(SkillRecord? skill, Fighter target) =>
        SkillElement(skill) is { } element && Overcomes(target.Element, element);

    /// <summary>Multiplies a damage value and rounds it.</summary>
    /// <param name="damage">The damage.</param>
    /// <param name="scale">The factor.</param>
    /// <returns>The rounded result, at least 1.</returns>
    public static int Scale(int damage, double scale) => Math.Max(1, (int)Math.Min(int.MaxValue, Math.Round(damage * scale)));

    /// <summary>The chance in percent that a damaging skill's status effect lands.</summary>
    public const int StatusHitChancePercent = 80;

    /// <summary>The damage poison does to a fighter each round.</summary>
    /// <param name="target">The poisoned fighter.</param>
    /// <returns>A tenth of the fighter's maximum HP, at least 1.</returns>
    public static int PoisonDamage(Fighter target) => Math.Max(1, target.MaxHp / 10);

    /// <summary>The experience one monster pays before the action factor.</summary>
    /// <param name="monsterLevel">The monster's level.</param>
    /// <param name="fighterLevel">The level of the fighter who earns it.</param>
    /// <param name="o">The battle options with the experience terms.</param>
    /// <returns>The base value from the monster's level and the level gap, scaled down when the fighter is far above the monster.</returns>
    public static double MonsterExp(int monsterLevel, int fighterLevel, BattleOptions o)
    {
        var under = Math.Max(0, monsterLevel - fighterLevel);
        var over = Math.Max(0, fighterLevel - monsterLevel);
        var k = Math.Max(1, o.MonsterExpBase + o.MonsterExpPerLevel * monsterLevel + o.ExpPerLevelUnderMonster * under - o.ExpPerLevelOverMonster * over);
        return k * LevelGapScale(fighterLevel, monsterLevel, o.LevelGapFullUpTo, o.LevelGapZeroFrom);
    }

    /// <summary>How much of a monster's experience a fighter earns, by what they did to it.</summary>
    /// <param name="monster">The monster.</param>
    /// <param name="fighter">The player or pet who may earn experience.</param>
    /// <param name="o">The battle options with the factors.</param>
    /// <param name="winPlate">The fighter who used the win plate, or null.</param>
    /// <returns>
    /// The factor for a killing blow (alone or in a combination), for the win plate user, or for a hit, plus a bonus per extra
    /// action; 0 when the fighter did nothing to it.
    /// </returns>
    public static double ActionFactor(Fighter monster, Fighter fighter, BattleOptions o, Fighter? winPlate = null)
    {
        var actions = monster.ActionsOf(fighter);
        var felling = monster.KilledBy.Where(f => f.IsHuman).Distinct().ToList();
        double first;
        if (felling.Contains(fighter))
            first = felling.Count >= 2 ? o.ExpComboKillFactor : o.ExpSoloKillFactor;
        else if (winPlate is not null && ReferenceEquals(winPlate, fighter) && monster.IsAlive)
            first = o.ExpSoloKillFactor;
        else if (actions > 0)
            first = o.ExpHitFactor;
        else
            return 0;
        return first + o.ExpPerExtraAction * Math.Max(0, actions - 1);
    }

    /// <summary>Adds up the experience a fighter earns from the monsters of a won battle.</summary>
    /// <param name="monsters">The monsters of the battle.</param>
    /// <param name="fighter">The player or pet who earns the experience.</param>
    /// <param name="fighterLevel">The level used for the level gap.</param>
    /// <param name="o">The battle options.</param>
    /// <param name="winPlate">The fighter who used the win plate, or null.</param>
    /// <returns>The whole experience points.</returns>
    public static int ExpShare(
        IEnumerable<Fighter> monsters,
        Fighter fighter,
        int fighterLevel,
        BattleOptions o,
        Fighter? winPlate = null)
    {
        var total = monsters.Where(m => m.Npc is not null && !m.Captured)
            .Sum(m => ActionFactor(m, fighter, o, winPlate) is var factor and > 0 ? factor * MonsterExp(m.Npc!.Level, fighterLevel, o) : 0);
        return (int)Math.Min(int.MaxValue, Math.Round(Math.Round(total, 6), MidpointRounding.AwayFromZero));
    }

    /// <summary>The fraction of an experience value kept when the fighter outlevels the monster.</summary>
    /// <param name="fighterLevel">The fighter's level.</param>
    /// <param name="monsterLevel">The monster's level.</param>
    /// <param name="fullUpTo">The level gap up to which the full value is kept.</param>
    /// <param name="zeroFrom">The level gap from which nothing is kept.</param>
    /// <returns>1 up to the first gap, 0 from the second, and a straight line between them.</returns>
    public static double LevelGapScale(int fighterLevel, int monsterLevel, int fullUpTo, int zeroFrom)
    {
        var gap = fighterLevel - monsterLevel;
        if (gap <= fullUpTo)
            return 1;
        return gap >= zeroFrom ? 0 : (double)(zeroFrom - gap) / (zeroFrom - fullUpTo);
    }

    /// <summary>The factor on the capture chance for the levels of the catcher and the monster.</summary>
    /// <param name="catcherLevel">The level of the catcher.</param>
    /// <param name="monsterLevel">The level of the monster.</param>
    /// <param name="o">The pet options with the step and the limits.</param>
    /// <returns>The minimum factor for a monster above the catcher, otherwise a factor that grows with the catcher's lead, within the limits.</returns>
    public static double CaptureLevelFactor(int catcherLevel, int monsterLevel, PetOptions o)
    {
        var min = o.CaptureLevelFactorMin;
        if (monsterLevel > catcherLevel)
            return min;
        return Math.Clamp(1 + o.CaptureLevelStep * (catcherLevel - monsterLevel), min, Math.Max(min, o.CaptureLevelFactorMax));
    }

    /// <summary>The chance in percent that a capture attempt catches a monster.</summary>
    /// <param name="hp">The monster's current HP.</param>
    /// <param name="maxHp">The monster's maximum HP.</param>
    /// <param name="catcherLevel">The level of the catcher.</param>
    /// <param name="monsterLevel">The level of the monster.</param>
    /// <param name="o">The pet options with the capture percentages.</param>
    /// <returns>The base percentage, raised the more HP the monster is missing, times <see cref="CaptureLevelFactor"/>.</returns>
    public static double CaptureChance(
        int hp,
        int maxHp,
        int catcherLevel,
        int monsterLevel,
        PetOptions o)
    {
        var missing = maxHp <= 0 ? 0 : Math.Clamp(1 - (double)hp / maxHp, 0, 1);
        return (o.CaptureBasePercent + o.CaptureHpPercent * missing) * CaptureLevelFactor(catcherLevel, monsterLevel, o);
    }

    /// <summary>The chance in percent that a flee attempt succeeds.</summary>
    /// <param name="level">The fleeing fighter's level.</param>
    /// <param name="speed">The fleeing fighter's speed.</param>
    /// <param name="enemyLevel">The enemies' average level.</param>
    /// <param name="enemySpeed">The enemies' average speed.</param>
    /// <param name="o">The battle options with the flee terms.</param>
    /// <returns>The base percentage times a factor that rises with the lead in speed and level, within the configured limits.</returns>
    public static double FleeChance(
        int level,
        int speed,
        double enemyLevel,
        double enemySpeed,
        BattleOptions o)
    {
        var s = Math.Clamp((speed - enemySpeed) / Math.Max(enemySpeed, 1), -1, 1);
        var min = o.FleeFactorMin;
        return o.FleeChancePercent * Math.Clamp(1 + o.FleeSpeedWeight * s + o.FleeLevelStep * (level - enemyLevel), min, Math.Max(min, o.FleeFactorMax));
    }

    /// <summary>The chance in percent that a fighter flees from the given enemies.</summary>
    /// <param name="fleeing">The fleeing fighter.</param>
    /// <param name="enemies">The enemies; their average level and speed are used (the fighter's own when there are none).</param>
    /// <param name="o">The battle options.</param>
    /// <returns>The flee chance in percent.</returns>
    public static double FleeChance(Fighter fleeing, IEnumerable<Fighter> enemies, BattleOptions o)
    {
        var standing = enemies.ToList();
        return standing.Count == 0
            ? FleeChance(fleeing.Level, fleeing.WindowSpeed, fleeing.Level, fleeing.WindowSpeed, o)
            : FleeChance(fleeing.Level, fleeing.WindowSpeed, standing.Average(e => (double)e.Level), standing.Average(e => (double)e.WindowSpeed), o);
    }

    /// <summary>Caps the experience multipliers so that their product stays within a limit.</summary>
    /// <param name="seasonal">The seasonal event multiplier.</param>
    /// <param name="potion">The multiplier of the player's potion or card.</param>
    /// <param name="cap">The largest product allowed; 0 or less means no cap.</param>
    /// <returns>The seasonal multiplier, limited to the cap, and the potion multiplier, limited so that the product stays within it.</returns>
    public static (double Seasonal, double Potion) CappedExpMultipliers(double seasonal, double potion, double cap)
    {
        if (cap <= 0)
            return (seasonal, potion);
        seasonal = Math.Min(seasonal, cap);
        return (seasonal, Math.Min(potion, cap / Math.Max(seasonal, double.Epsilon)));
    }

    /// <summary>Tells whether a winner side gets a drop roll from a monster.</summary>
    /// <param name="monster">The defeated monster.</param>
    /// <param name="winnerSide">The fighters of the winning side.</param>
    /// <returns>True when <see cref="DropRollsOf"/> is above 0.</returns>
    public static bool RollsDropOf(Fighter monster, IReadOnlyCollection<Fighter> winnerSide) => DropRollsOf(monster, winnerSide) > 0;

    /// <summary>Counts the copies of a monster's drop a winner side gets.</summary>
    /// <param name="monster">The defeated monster.</param>
    /// <param name="winnerSide">The fighters of the winning side.</param>
    /// <returns>One when nobody felled the monster, otherwise the number of distinct fighters of the winner side in the felling blow.</returns>
    public static int DropRollsOf(Fighter monster, IReadOnlyCollection<Fighter> winnerSide) =>
        monster.KilledBy.Count == 0 ? 1 : monster.KilledBy.Distinct().Count(winnerSide.Contains);

    /// <summary>The experience a player gets from their pet's experience after buffing it.</summary>
    /// <param name="petExp">The experience the pet earned in the battle.</param>
    /// <param name="percent">The share in percent.</param>
    /// <returns>The share rounded to a whole point, 0 for a negative percentage.</returns>
    public static double BuffShare(int petExp, double percent) => Math.Round(petExp * Math.Max(0, percent) / 100, MidpointRounding.AwayFromZero);

    /// <summary>The experience a fighter earns from the combination damage dealt to the monsters (bursting).</summary>
    /// <param name="monsters">The monsters of the battle.</param>
    /// <param name="fighter">The player or pet who earns it.</param>
    /// <param name="fighterLevel">The level used for the maximum level check.</param>
    /// <param name="o">The battle options.</param>
    /// <returns>The burst experience, 0 when the fighter is above the maximum level, reborn or a pet not on the burstable list.</returns>
    public static double BurstDamageExp(IEnumerable<Fighter> monsters, Fighter fighter, int fighterLevel, BattleOptions o)
    {
        if (fighterLevel > o.BurstMaxLevel || fighter.Reborn > 0 || (fighter.Kind == 4 && !IsBurstablePet(fighter.Npc, o))) // pet
            return 0;
        var share = monsters.Where(m => m.Npc is not null && !m.Captured).Sum(m => m.ComboDamage * BurstDamagePercentOf(m, o)) / 100;
        return fighter.Kind == 4 ? share * Math.Max(0, o.BurstablePetPercent) / 100 : share; // pet
    }

    /// <summary>The percentage of a monster's combination damage that is paid as burst experience.</summary>
    /// <param name="monster">The defeated monster.</param>
    /// <param name="o">The battle options with the base, the step and the maximum steps.</param>
    /// <returns>The base percentage plus a step for each distinct debuff the monster carried when it fell, never below 0.</returns>
    public static double BurstDamagePercentOf(Fighter monster, BattleOptions o) =>
        Math.Max(
            0,
            o.BurstDebuffBasePercent
                + o.BurstDebuffStepPercent * Math.Min((monster.DebuffsAtDeath ?? StatusRules.DebuffEffects(monster)).Count, Math.Max(0, o.BurstDebuffSteps)));

    /// <summary>Tells whether a pet is on the burstable list.</summary>
    /// <param name="npc">The pet's record, or null.</param>
    /// <param name="o">The battle options with the list of ids and names.</param>
    /// <returns>True when the pet's id or name is listed.</returns>
    public static bool IsBurstablePet(NpcRecord? npc, BattleOptions o) =>
        npc is not null && o.BurstablePets.Split([',', ';'], StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
            .Any(e => ushort.TryParse(e, out var id) ? id == npc.Id : string.Equals(e, npc.Name, StringComparison.OrdinalIgnoreCase));

    /// <summary>Rolls the gold a monster pays.</summary>
    /// <param name="npc">The monster's record.</param>
    /// <param name="rng">The random source.</param>
    /// <returns>Three times the monster's level plus a roll from 0 to 5.</returns>
    public static int GoldReward(NpcRecord npc, Random rng) => Math.Max(1, (int)npc.Level) * 3 + rng.Next(0, 6);

    /// <summary>Rolls whether a monster drops an item and which one.</summary>
    /// <param name="npc">The monster's record with its item list.</param>
    /// <param name="chancePercent">The chance in percent that anything drops.</param>
    /// <param name="rng">The random source.</param>
    /// <param name="rank">Gives the rank of an item id.</param>
    /// <returns>The id of the dropped item, or null.</returns>
    public static ushort? Drop(NpcRecord npc, int chancePercent, Random rng, Func<ushort, int> rank)
    {
        if (npc.Items.Count == 0 || rng.Next(100) >= chancePercent)
            return null;
        var top = npc.Items.Max(i => Math.Max(1, rank(i)));
        var weights = npc.Items.Select(i => DropWeight(Math.Max(1, rank(i)), top)).ToArray();
        var roll = rng.NextDouble() * weights.Sum();
        for (var i = 0; i < weights.Length; i++)
            if ((roll -= weights[i]) < 0)
                return npc.Items[i];
        return npc.Items[^1];
    }

    /// <summary>Rolls a drop from a calendar event's drop table.</summary>
    /// <param name="table">The rows with an item and a percent each.</param>
    /// <param name="rng">The random source.</param>
    /// <returns>The item id of the row hit, or null when none is.</returns>
    public static ushort? EventDrop(IReadOnlyList<CalendarDrop> table, Random rng)
    {
        var rows = table.Where(d => d.Percent > 0).ToList();
        var total = rows.Sum(d => d.Percent);
        if (rows.Count == 0)
            return null;
        var roll = rng.NextDouble() * Math.Max(100, total);
        foreach (var row in rows)
            if ((roll -= row.Percent) < 0)
                return row.Item;
        return null;
    }

    /// <summary>The weight of a drop slot in the drop roll.</summary>
    /// <param name="rank">The item's rank.</param>
    /// <param name="top">The highest rank on the monster's list.</param>
    /// <returns>Two to the power of the rank's distance below the top.</returns>
    public static double DropWeight(int rank, int top) => Math.Pow(2, Math.Max(0, top - rank));

    /// <summary>Rolls the damage of a magic damage skill.</summary>
    /// <param name="attacker">The casting fighter.</param>
    /// <param name="target">The fighter hit.</param>
    /// <param name="rng">The random source.</param>
    /// <param name="power">An amount added to the base damage.</param>
    /// <returns>The damage of <see cref="SkillHit"/> without a critical hit.</returns>
    public static int SkillDamage(Fighter attacker, Fighter target, Random rng, int power = 0) => SkillHit(attacker, target, rng, power, false).Damage;

    /// <summary>Rolls a magic hit from intelligence, level and worn magic attack against wisdom and worn magic defence.</summary>
    /// <param name="attacker">The casting fighter.</param>
    /// <param name="target">The fighter hit.</param>
    /// <param name="rng">The random source.</param>
    /// <param name="power">An amount added to the base damage.</param>
    /// <param name="canCritical">True when the spell may be a critical hit.</param>
    /// <returns>The damage, at least 1, and whether the hit was a critical hit.</returns>
    public static DamageRoll SkillHit(
        Fighter attacker,
        Fighter target,
        Random rng,
        int power,
        bool canCritical)
    {
        var baseDamage = Math.Max(1, attacker.Intelligence * 2 + attacker.TotalLevel * 2 + attacker.MagicAttack - target.Wisdom - target.MagicDefence + power);
        var critical = canCritical && attacker.CriticalPercent > 0 && rng.Next(100) < attacker.CriticalPercent;
        return new DamageRoll(
            Scaled(
                baseDamage * (critical ? 2 : 1) * (0.9 + rng.NextDouble() * 0.2) * JobScale(attacker, Career.Wit) / JobScale(target, Career.Priest),
                attacker,
                target),
            critical);
    }

    /// <summary>Rolls a physical damage skill as a physical hit with 1.5 times the damage.</summary>
    /// <param name="attacker">The attacking fighter.</param>
    /// <param name="target">The fighter hit.</param>
    /// <param name="rng">The random source.</param>
    /// <returns>The hit with its damage raised by half.</returns>
    public static DamageRoll PhysicalSkillHit(Fighter attacker, Fighter target, Random rng)
    {
        var hit = PhysicalHit(attacker, target, rng);
        return hit with { Damage = Math.Max(1, (int)Math.Round(hit.Damage * 1.5)) };
    }

    /// <summary>The power a skill adds to its damage.</summary>
    /// <param name="skill">The skill, or null.</param>
    /// <param name="o">The battle options with the list of skill ids that use their power.</param>
    /// <returns>The skill's power when it is listed, otherwise 0.</returns>
    public static int PowerTerm(SkillRecord? skill, BattleOptions o) =>
        skill is { Power: > 0 } && IsListed(o.PowerDamageSkills, skill.Id) ? skill.Power : 0;

    /// <summary>Tells whether an id is in a comma or semicolon separated list.</summary>
    /// <param name="list">The list of ids.</param>
    /// <param name="id">The id to look for.</param>
    /// <returns>True when the id is listed.</returns>
    public static bool IsListed(string list, ushort id) =>
        list.Split([',', ';'], StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries).Any(e => ushort.TryParse(e, out var n) && n == id);

    /// <summary>The chance in percent that a monster that knows skills uses one instead of a plain attack.</summary>
    public const int MonsterSkillChancePercent = 50;

    /// <summary>The damage of a hit against a defending target.</summary>
    /// <param name="damage">The damage before defending.</param>
    /// <returns>Half the damage, at least 1.</returns>
    public static int Defended(int damage) => Math.Max(1, damage / 2);

    /// <summary>Sorts the fighters into the order they act in.</summary>
    /// <param name="fighters">The fighters.</param>
    /// <returns>The living fighters, fastest first, the player side first among equal speeds.</returns>
    public static List<Fighter> TurnOrder(IEnumerable<Fighter> fighters) =>
        [.. fighters.Where(f => f.IsAlive).OrderByDescending(TurnSpeed).ThenByDescending(f => f.IsPlayerSide)];

    /// <summary>The speed that decides a fighter's place in the turn order.</summary>
    /// <param name="f">The fighter.</param>
    /// <returns>The window speed scaled by the fighter's agility status effects.</returns>
    public static double TurnSpeed(Fighter f) => f.WindowSpeed * StatusRules.AgilityScale(f);
}
