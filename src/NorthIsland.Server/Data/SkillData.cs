using System.Buffers.Binary;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NorthIsland.GameData.Formulas;
using NorthIsland.GameData.Skills;
using NorthIsland.Protocol.Messages.Player;

namespace NorthIsland.Server.Data;

/// <summary>
/// The skill table (Skill.dat) and skill animations (SkillData.MBTM), loaded on first use, and the rules for learning and training skills.
/// </summary>
public sealed class SkillData
{
    /// <summary>The skill type of the life skills (gathering, facture, fishing and alchemy).</summary>
    public const byte LifeSkillType = 13;

    /// <summary>The skill table, or null when Skill.dat is missing; loaded on first access.</summary>
    private readonly Lazy<SkillTable?> _table;

    /// <summary>The skill animation table, or null when SkillData.MBTM is missing; loaded on first access.</summary>
    private readonly Lazy<SkillMotionTable?> _motions;

    /// <summary>Sets up lazy loading of Skill.dat and SkillData.MBTM from the game data directory.</summary>
    /// <param name="options">Server options; they name the game data directory.</param>
    /// <param name="logger">Logs what was loaded or missing.</param>
    public SkillData(
        IOptions<ServerOptions> options,
        ILogger<SkillData> logger)
    {
        var motionPath = Path.Combine(ServerPaths.Resolve(options.Value.GameDataDirectory), "SkillData.MBTM");
        _motions = new Lazy<SkillMotionTable?>(() =>
        {
            if (!File.Exists(motionPath))
            {
                logger.LogWarning("Skill animations not found at {Path}; battle actions use a fixed delay", motionPath);
                return null;
            }
            var table = SkillMotionTable.Open(motionPath);
            logger.LogInformation("Loaded {Count} skill animations from {Path}", table.Count, motionPath);
            return table;
        });
        var path = Path.Combine(ServerPaths.Resolve(options.Value.GameDataDirectory), "Skill.dat");
        _table = new Lazy<SkillTable?>(() =>
        {
            if (!File.Exists(path))
            {
                logger.LogWarning("Skill data not found at {Path}; monsters only use Hand Attack", path);
                return null;
            }
            var table = SkillTable.Open(path);
            logger.LogInformation("Loaded {Count} skills from {Path}", table.Count, path);
            return table;
        });
    }

    /// <summary>Looks up a skill; null when unknown or without Skill.dat.</summary>
    /// <param name="skillId">The skill id.</param>
    public SkillRecord? this[ushort skillId] => _table.Value?[skillId];

    /// <summary>The skill animation table, null without SkillData.MBTM.</summary>
    public SkillMotionTable? Motions => _motions.Value;

    /// <summary>The skill table (Skill.dat), loaded on first access.</summary>
    public SkillTable? Table => _table.Value;

    /// <summary>
    /// The root cells of a skill tree with the character level at which each is learned, as (cell, level) pairs.
    /// </summary>
    public static readonly IReadOnlyList<(int Cell, int Level)> RootLevels = [(0, 2), (6, 3), (12, 4)];

    /// <summary>Tells whether a cell of the skill tree is a root cell, one of the cells 0, 6, 12 and so on.</summary>
    /// <param name="cell">The cell index in the skill tree.</param>
    /// <returns>True for a root cell.</returns>
    public static bool IsRootCell(int cell) => cell % 6 == 0;

    /// <summary>Tells whether the character knows the skill of a cell or any later skill of its chain.</summary>
    /// <param name="c">The character.</param>
    /// <param name="cellSkill">The skill id of the cell.</param>
    /// <returns>True when the character knows a skill of the chain.</returns>
    public bool KnowsChainOf(Character c, ushort cellSkill) =>
        Table is { } t ? t.ChainFrom(cellSkill).Exists(id => c.Skill(id) is not null) : c.Skill(cellSkill) is not null;

    /// <summary>Teaches the root skills whose level the character has just reached.</summary>
    /// <param name="c">The character; learned skills are added to it.</param>
    /// <param name="levelBefore">The character's level before the level-up.</param>
    /// <returns>The skills that were learned.</returns>
    public List<SkillRecord> LearnRoots(Character c, int levelBefore)
    {
        List<SkillRecord> learned = [];
        var cells = SkillTree.For(c.Element);
        if (cells.Count == 0)
            return learned;
        foreach (var (cell, level) in RootLevels)
        {
            var id = cells[cell].SkillId;
            if (levelBefore >= level || c.Level < level || this[id] is not { } skill || KnowsChainOf(c, id))
                continue;
            c.Skills.Add(new LearnedSkill { Id = id });
            learned.Add(skill);
        }
        return learned;
    }

    /// <summary>
    /// Teaches the non-root skills of the character's element whose stat requirements the character now meets.
    /// </summary>
    /// <param name="c">The character; learned skills are added to it.</param>
    /// <returns>The skills that were learned.</returns>
    public List<SkillRecord> LearnCells(Character c)
    {
        List<SkillRecord> learned = [];
        var cells = SkillTree.For(c.Element);
        for (var i = 0; i < cells.Count; i++)
        {
            var cell = cells[i];
            if (IsRootCell(i) || this[cell.SkillId] is not { } skill || KnowsChainOf(c, cell.SkillId)
                || !cell.IsMetBy(c.Strength, c.Constitution, c.Intelligence, c.Wisdom, c.Agility))
                continue;
            c.Skills.Add(new LearnedSkill { Id = cell.SkillId });
            learned.Add(skill);
        }
        return learned;
    }

    /// <summary>Removes the skills of every cell whose stat requirements the character no longer meets.</summary>
    /// <param name="c">The character; forgotten skills are removed from it.</param>
    /// <returns>The ids of the skills that were removed.</returns>
    public List<ushort> ForgetLost(Character c)
    {
        List<ushort> lost = [];
        foreach (var cell in SkillTree.For(c.Element))
        {
            if (cell.IsMetBy(c.Strength, c.Constitution, c.Intelligence, c.Wisdom, c.Agility))
                continue;
            foreach (var id in Table?.ChainFrom(cell.SkillId) ?? [cell.SkillId])
                if (c.Skill(id) is { } known && c.Skills.Remove(known))
                    lost.Add(id);
        }
        return lost;
    }

    /// <summary>
    /// Adds skill experience, stopping at the experience of the next level, or at the cap for the maximum level;
    /// the level follows the experience but never drops.
    /// </summary>
    /// <param name="f">The formula table giving the experience per skill level.</param>
    /// <param name="exp">The skill's experience before.</param>
    /// <param name="level">The skill's level before.</param>
    /// <param name="gain">The experience to add.</param>
    /// <param name="maxLevel">The skill's maximum level; with 1 or less the skill does not grow.</param>
    /// <returns>The experience and level after the gain.</returns>
    public static (int Exp, byte Level) Grow(
        FormulaTable f,
        int exp,
        byte level,
        int gain,
        byte maxLevel)
    {
        if (maxLevel <= 1 || gain <= 0)
            return (exp, level);
        var cap = f.TotalSkillLevelExp(maxLevel);
        if (exp >= cap)
            return (exp, level);
        var from = Math.Max(level, f.SkillLevelFromExp(Math.Max(0, exp), maxLevel));
        var limit = Math.Min(cap, f.TotalSkillLevelExp(Math.Min(from + 1, maxLevel)));
        var total = (int)Math.Min(limit, (long)Math.Max(0, exp) + gain);
        return (total, (byte)Math.Max(level, f.SkillLevelFromExp(total, maxLevel)));
    }

    /// <summary>
    /// Works out the skill experience for one attack: the target's level, scaled by the share of its maximum HP the damage took, at least 1.
    /// </summary>
    /// <param name="targetLevel">The level of the target.</param>
    /// <param name="targetMaxHp">The target's maximum HP; with 0 or less the full level counts.</param>
    /// <param name="damage">The damage dealt.</param>
    /// <returns>The experience gained.</returns>
    public static int BlowExp(int targetLevel, int targetMaxHp, int damage)
    {
        var level = Math.Max(1, targetLevel);
        if (targetMaxHp <= 0)
            return level;
        return Math.Max(1, (int)Math.Round(level * Math.Min(1.0, Math.Max(0, damage) / (double)targetMaxHp)));
    }

    /// <summary>
    /// Trains a skill the character knows; when it reaches its maximum level, its successor skill is learned.
    /// </summary>
    /// <param name="c">The character.</param>
    /// <param name="f">The formula table giving the experience per skill level.</param>
    /// <param name="skillId">The skill to train.</param>
    /// <param name="gain">The experience to add.</param>
    /// <returns>The result, or null when the skill is not trainable, not known, or nothing changed.</returns>
    public SkillTraining? Train(Character c, FormulaTable f, ushort skillId, int gain)
    {
        if (this[skillId] is not { } skill || skill is not ({ Tree: >= 1 and <= 4 } or { Type: LifeSkillType }) || c.Skill(skillId) is not { } learned)
            return null;
        var (exp, level) = Grow(f, learned.Exp, learned.Level, gain, skill.MaxLevel);
        if (exp == learned.Exp && level == learned.Level)
            return null;
        var rose = level > learned.Level;
        (learned.Exp, learned.Level) = (exp, level);
        SkillRecord? successor = null;
        if (rose && level >= skill.MaxLevel && Table?.SuccessorOf(skillId) is { } next && c.Skill(next.Id) is null)
        {
            c.Skills.Add(new LearnedSkill { Id = next.Id });
            successor = next;
        }
        return new SkillTraining(exp, level, rose, successor);
    }

    /// <summary>Trains one of a pet's three skills.</summary>
    /// <param name="pet">The pet; its skill level and experience are updated.</param>
    /// <param name="slots">The skill ids of the pet's three skill slots.</param>
    /// <param name="skill">The skill to train.</param>
    /// <param name="f">The formula table giving the experience per skill level.</param>
    /// <param name="gain">The experience to add.</param>
    /// <returns>The result, or null when the pet has no such skill at a level above 0 or nothing changed.</returns>
    public static SkillTraining? TrainPet(
        Pet pet,
        IReadOnlyList<ushort> slots,
        SkillRecord skill,
        FormulaTable f,
        int gain)
    {
        var i = -1;
        for (var k = 0; k < Math.Min(3, slots.Count); k++)
            if (slots[k] == skill.Id && pet.SkillLevels.ElementAtOrDefault(k) > 0)
            {
                i = k;
                break;
            }
        if (i < 0)
            return null;
        if (pet.SkillExp.Length < 3)
            pet.SkillExp = [.. pet.SkillExp, .. new int[3 - pet.SkillExp.Length]];
        var (exp, level) = Grow(f, pet.SkillExp[i], pet.SkillLevels[i], gain, skill.MaxLevel);
        if (exp == pet.SkillExp[i] && level == pet.SkillLevels[i])
            return null;
        var rose = level > pet.SkillLevels[i];
        (pet.SkillExp[i], pet.SkillLevels[i]) = (exp, level);
        return new SkillTraining(exp, level, rose, null);
    }

    /// <summary>Builds the skill records of the skill list packet <c>5/3</c> from the skills the character knows.</summary>
    /// <param name="c">The character.</param>
    /// <returns>One entry per known skill that has a Skill.dat key.</returns>
    public List<SkillEntry> Entries(Character c)
    {
        List<SkillEntry> entries = [];
        foreach (var s in c.Skills)
        {
            if (this[s.Id] is not { Key: > 0 } skill)
                continue;
            var data = new byte[SkillEntry.DataSize];
            data[0] = s.Level;
            BinaryPrimitives.WriteInt32LittleEndian(data.AsSpan(1), s.Exp);
            entries.Add(new SkillEntry(skill.Key, data));
        }
        return entries;
    }
}
