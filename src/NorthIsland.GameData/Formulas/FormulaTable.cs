using System.Buffers.Binary;

namespace NorthIsland.GameData.Formulas;

/// <summary>
/// The constants of the client's Formula.Dat and the level, experience, HP, SP and combat value formulas built on them.
/// </summary>
public sealed class FormulaTable
{
    /// <summary>Size of the Formula.Dat record in bytes.</summary>
    public const int RecordSize = 407;

    /// <summary>Value the first byte of Formula.Dat must have.</summary>
    public const byte FormatVersion = 2;

    /// <summary>Exponent of the experience curve: the exp for a level is the level to this power plus <see cref="ExpAdd"/>.</summary>
    public double ExpPower { get; init; }

    /// <summary>Constant added to the experience for a level and to the skill experience for a level.</summary>
    public int ExpAdd { get; init; }

    /// <summary>HP added per point of total level in <see cref="MaxHp"/>.</summary>
    public double HpPerLevel { get; init; }

    /// <summary>Factor on constitution in the level-power term of <see cref="MaxHp"/>.</summary>
    public double HpStatScale { get; init; }

    /// <summary>Exponent applied to the total level in the level-power term of <see cref="MaxHp"/>.</summary>
    public double HpLevelPower { get; init; }

    /// <summary>HP added per point of constitution in <see cref="MaxHp"/>.</summary>
    public double HpPerStat { get; init; }

    /// <summary>Flat HP added in <see cref="MaxHp"/>.</summary>
    public ushort HpBase { get; init; }

    /// <summary>SP added per point of total level in <see cref="MaxSp"/>.</summary>
    public double SpPerLevel { get; init; }

    /// <summary>Factor on wisdom in the level-power term of <see cref="MaxSp"/>.</summary>
    public double SpStatScale { get; init; }

    /// <summary>Exponent applied to the total level in the level-power term of <see cref="MaxSp"/>.</summary>
    public double SpLevelPower { get; init; }

    /// <summary>SP added per point of wisdom in <see cref="MaxSp"/>.</summary>
    public double SpPerStat { get; init; }

    /// <summary>Flat SP added in <see cref="MaxSp"/>.</summary>
    public ushort SpBase { get; init; }

    /// <summary>
    /// The 20 factors <see cref="CombatValue"/> uses, four per value in the order ATK, DEF, MATK, MDEF, SPD; empty for a
    /// default instance.
    /// </summary>
    public IReadOnlyList<double> CombatFactors { get; init; } = [];

    /// <summary>Reads and parses a Formula.Dat file.</summary>
    /// <param name="path">Path of the file.</param>
    /// <returns>The parsed table.</returns>
    public static FormulaTable Open(string path) => Parse(File.ReadAllBytes(path));

    /// <summary>
    /// Parses the contents of a Formula.Dat file; throws <see cref="InvalidDataException"/> when it is too short or has the
    /// wrong format byte.
    /// </summary>
    /// <param name="data">The raw file contents.</param>
    /// <returns>The parsed table.</returns>
    public static FormulaTable Parse(ReadOnlySpan<byte> data)
    {
        if (data.Length < RecordSize)
            throw new InvalidDataException($"Formula.Dat has {data.Length} bytes, expected {RecordSize}.");
        if (data[0] != FormatVersion)
            throw new InvalidDataException($"Formula.Dat format {data[0]}, expected {FormatVersion}.");
        return new FormulaTable
        {
            ExpPower = D(data, 241),
            HpPerLevel = D(data, 249),
            HpStatScale = D(data, 257),
            HpLevelPower = D(data, 265),
            HpPerStat = D(data, 273),
            SpPerLevel = D(data, 281),
            SpStatScale = D(data, 289),
            SpLevelPower = D(data, 297),
            SpPerStat = D(data, 305),
            ExpAdd = BinaryPrimitives.ReadInt32LittleEndian(data.Slice(361, 4)),
            HpBase = BinaryPrimitives.ReadUInt16LittleEndian(data.Slice(365, 2)),
            SpBase = BinaryPrimitives.ReadUInt16LittleEndian(data.Slice(367, 2)),
            CombatFactors = Factors(data),
        };
    }

    /// <summary>Calculates the ATK, DEF, MATK, MDEF or SPD value of a character from its level, attribute and item bonus.</summary>
    /// <param name="index">Which value: 0 ATK, 1 DEF, 2 MATK, 3 MDEF, 4 SPD.</param>
    /// <param name="totalLevel">Reborn count * 100 + level.</param>
    /// <param name="stat">The attribute value the combat value is based on.</param>
    /// <param name="bonus">Sum of the item bonuses for this value.</param>
    /// <param name="elementMatches">True when the element condition holds, which adds the fourth factor times the total level.</param>
    /// <param name="jobMatches">True when the job condition holds, which adds 10 % of the own part (level and attribute terms).</param>
    /// <param name="ridingExtra">Flat amount added before rounding.</param>
    /// <returns>The value rounded half up, never below 0; 0 when the table has no combat factors.</returns>
    public int CombatValue(
        int index,
        int totalLevel,
        int stat,
        int bonus,
        bool elementMatches,
        bool jobMatches,
        int ridingExtra = 0)
    {
        if (CombatFactors.Count < 20)
            return 0;
        var f0 = CombatFactors[4 * index];
        var f1 = CombatFactors[4 * index + 1];
        var f2 = CombatFactors[4 * index + 2];
        var f3 = CombatFactors[4 * index + 3];
        var e = elementMatches ? 1 : 0;
        var own = totalLevel * f1 + stat * f0 + totalLevel * (e * f3);
        var value = RoundHalfUp(own + bonus * f2 + ridingExtra);
        if (jobMatches)
            value += RoundHalfUp(own * 0.1);
        return Math.Max(value, 0);
    }

    /// <summary>Rounds half up: the next integer up when the fraction is at least 0.5, else down.</summary>
    /// <param name="value">The value to round.</param>
    /// <returns>The rounded integer.</returns>
    public static int RoundHalfUp(double value) =>
        (int)(value - Math.Truncate(value) >= 0.5 ? Math.Ceiling(value) : Math.Floor(value));

    /// <summary>Calculates the maximum HP from the total level and constitution.</summary>
    /// <param name="totalLevel">Reborn count * 100 + level.</param>
    /// <param name="constitution">The constitution attribute.</param>
    /// <param name="bonus">Bonus HP added to the result.</param>
    /// <param name="extra">Further flat HP added to the result.</param>
    /// <returns>The maximum HP, never below 0.</returns>
    public int MaxHp(int totalLevel, int constitution, int bonus, int extra)
    {
        var result = (int)Math.Round(Math.Pow(totalLevel, HpLevelPower) * (constitution * HpStatScale)
            + totalLevel * HpPerLevel + constitution * HpPerStat)
            + HpBase + bonus + extra;
        return Math.Max(result, 0);
    }

    /// <summary>Calculates the maximum SP from the total level and wisdom.</summary>
    /// <param name="totalLevel">Reborn count * 100 + level.</param>
    /// <param name="wisdom">The wisdom attribute.</param>
    /// <param name="bonus">Bonus SP added to the result.</param>
    /// <param name="extra">Further flat SP added to the result.</param>
    /// <returns>The maximum SP.</returns>
    public int MaxSp(int totalLevel, int wisdom, int bonus, int extra) =>
        (int)Math.Round(Math.Pow(totalLevel, SpLevelPower) * (wisdom * SpStatScale)
            + totalLevel * SpPerLevel + wisdom * SpPerStat)
            + SpBase + bonus + extra;

    /// <summary>Calculates the experience needed to gain one level.</summary>
    /// <param name="level">The level that is gained.</param>
    /// <param name="reborn">Reborn count; 0 uses the table constants, any other value a fixed curve.</param>
    /// <returns>The experience for that single level.</returns>
    public int LevelUpExp(int level, int reborn)
    {
        double power, add;
        if (reborn == 0)
        {
            power = ExpPower;
            add = ExpAdd;
        }
        else
        {
            power = 3.3;
            add = level <= 150 ? 50 : Math.Pow(level - 150, 4.9);
        }
        return (int)Math.Round(Math.Pow(level, power) + add);
    }

    /// <summary>
    /// Calculates the total experience at which a level is reached, the sum of <see cref="LevelUpExp"/> for levels 1 to the
    /// given one (wraps on overflow).
    /// </summary>
    /// <param name="level">The level to reach.</param>
    /// <param name="reborn">Reborn count.</param>
    /// <returns>The cumulative experience.</returns>
    public int TotalLevelExp(int level, int reborn)
    {
        var total = 0;
        for (var i = 1; i <= level; i++)
            total = unchecked(total + LevelUpExp(i, reborn));
        return total;
    }

    /// <summary>
    /// Finds the level that a total experience corresponds to, counting up from a start level and stopping at <see
    /// cref="MaxLevel"/>.
    /// </summary>
    /// <param name="exp">The total experience.</param>
    /// <param name="reborn">Reborn count.</param>
    /// <param name="currentLevel">The level to start counting from.</param>
    /// <returns>The highest level whose cumulative experience does not exceed <paramref name="exp"/>.</returns>
    public int LevelFromExp(int exp, int reborn, int currentLevel = 1)
    {
        var level = currentLevel;
        for (var guard = 0; guard < 65535; guard++)
        {
            level++;
            if (TotalLevelExp(level, reborn) > exp || MaxLevel(reborn) < level)
                break;
        }
        return level - 1;
    }

    /// <summary>Calculates the skill experience needed for one skill level, 0 for level 1.</summary>
    /// <param name="level">The skill level.</param>
    /// <returns>The experience from the previous level to this one.</returns>
    public int SkillLevelExp(int level) => level <= 1 ? 0 : (int)Math.Round(Math.Pow(level, ExpPower)) + ExpAdd;

    /// <summary>Calculates the cumulative skill experience at which a skill level is reached.</summary>
    /// <param name="level">The skill level.</param>
    /// <returns>The sum of <see cref="SkillLevelExp"/> for levels 1 to the given one, capped at the largest int.</returns>
    public int TotalSkillLevelExp(int level)
    {
        var total = 0L;
        for (var i = 1; i <= level; i++)
            total += SkillLevelExp(i);
        return (int)Math.Min(int.MaxValue, total);
    }

    /// <summary>Finds the skill level that a cumulative skill experience corresponds to.</summary>
    /// <param name="exp">The cumulative skill experience.</param>
    /// <param name="maxLevel">The highest level the skill can have.</param>
    /// <returns>The skill level, from 1 up to <paramref name="maxLevel"/>.</returns>
    public int SkillLevelFromExp(int exp, int maxLevel)
    {
        var level = 1;
        while (level < maxLevel && TotalSkillLevelExp(level + 1) <= exp)
            level++;
        return level;
    }

    /// <summary>Returns the highest character level for a reborn count.</summary>
    /// <param name="reborn">Reborn count.</param>
    /// <returns>190 when reborn, else 199.</returns>
    public static int MaxLevel(int reborn) => reborn > 0 ? 190 : 199;

    /// <summary>Combines a reborn count and a level into the total level the formulas use.</summary>
    /// <param name="level">The character level.</param>
    /// <param name="reborn">Reborn count.</param>
    /// <returns>Reborn count * 100 + level.</returns>
    public static int TotalLevel(int level, int reborn) => reborn * 100 + level;

    /// <summary>Reads the 20 combat factors, 8-byte doubles starting at offset 1.</summary>
    /// <param name="data">The raw file contents.</param>
    /// <returns>The factors.</returns>
    private static double[] Factors(ReadOnlySpan<byte> data)
    {
        var factors = new double[20];
        for (var i = 0; i < factors.Length; i++)
            factors[i] = D(data, 1 + 8 * i);
        return factors;
    }

    /// <summary>Reads a little-endian double.</summary>
    /// <param name="data">The raw file contents.</param>
    /// <param name="offset">Byte offset of the value.</param>
    /// <returns>The value.</returns>
    private static double D(ReadOnlySpan<byte> data, int offset) => BinaryPrimitives.ReadDoubleLittleEndian(data.Slice(offset, 8));
}
