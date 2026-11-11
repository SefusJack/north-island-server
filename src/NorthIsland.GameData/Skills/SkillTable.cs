using System.Buffers.Binary;
using NorthIsland.GameData.Events;

namespace NorthIsland.GameData.Skills;

/// <summary>The skills of Skill.dat by id.</summary>
public sealed class SkillTable
{
    /// <summary>The size in bytes of one record.</summary>
    public const int RecordSize = 148;

    /// <summary>The skill id of the bare-hand normal attack.</summary>
    public const ushort HandAttack = 10001; // Hand Attack

    /// <summary>The skill id of the capture action.</summary>
    public const ushort Capture = 10008; // Capture

    /// <summary>The skill id of Fervidity, which can mess its target.</summary>
    public const ushort Fervidity = 25067; // Fervidity

    /// <summary>The skill id of Hot Fire Attack, whose status counts as an attack buff.</summary>
    public const ushort HotFireAttack = 15189; // Hot Fire Attack

    /// <summary>The action id sent when a fighter uses an item in battle.</summary>
    public const ushort ItemUsage = 10024; // Item Usage

    /// <summary>The action id of guarding.</summary>
    public const ushort Defense = 60021; // Defense

    /// <summary>The skill id of fleeing from a battle.</summary>
    public const ushort Flee = 60041; // Flee

    /// <summary>The action id sent when an attempt to flee fails.</summary>
    public const ushort FailToFlee = 60046; // Fail to Flee

    /// <summary>Whether a skill id is one of the normal weapon attacks.</summary>
    /// <param name="id">The skill id.</param>
    /// <returns>True when the id is a normal attack.</returns>
    public static bool IsNormalAttack(ushort id) => id is HandAttack or (>= 10002 and <= 10005) // 10002 Blade Attack, 10005 Sword Attack
        or 10007 // Gun Attack
        or 10018 // NPC Gun Attack
        or 10019 // Bow Attack
        or 10020 // NPC Bow Attack
        or 10026 // Bazooka Attack
        or 10027; // NPC Bazooka Attack

    /// <summary>The skills by id.</summary>
    private readonly Dictionary<ushort, SkillRecord> _byId;

    /// <summary>The skill that follows each skill in the chain of prerequisites.</summary>
    private readonly Dictionary<ushort, SkillRecord> _successors = [];

    /// <summary>Wraps the parsed skills and builds the successor map from their prerequisites.</summary>
    /// <param name="byId">The skills by id.</param>
    private SkillTable(
        Dictionary<ushort, SkillRecord> byId)
    {
        _byId = byId;
        foreach (var skill in byId.Values.Where(s => s.PrereqSkillId != 0).OrderBy(s => s.Id))
            _successors.TryAdd(skill.PrereqSkillId, skill);
    }

    /// <summary>The number of skills.</summary>
    public int Count => _byId.Count;

    /// <summary>The skill with an id, or null.</summary>
    /// <param name="id">The skill id.</param>
    public SkillRecord? this[ushort id] => _byId.GetValueOrDefault(id);

    /// <summary>The skill that has the given skill as its prerequisite (the lowest id when there are several).</summary>
    /// <param name="id">The prerequisite skill id.</param>
    /// <returns>The successor skill, or null.</returns>
    public SkillRecord? SuccessorOf(ushort id) => _successors.GetValueOrDefault(id);

    /// <summary>The chain of skills that follow one another through their prerequisites, starting at a skill.</summary>
    /// <param name="id">The skill id to start from.</param>
    /// <returns>The skill ids, starting with <paramref name="id"/>.</returns>
    public List<ushort> ChainFrom(ushort id)
    {
        List<ushort> chain = [id];
        while (SuccessorOf(chain[^1]) is { } next && !chain.Contains(next.Id))
            chain.Add(next.Id);
        return chain;
    }

    /// <summary>All skills.</summary>
    public IEnumerable<SkillRecord> All => _byId.Values;

    /// <summary>Reads and parses a Skill.dat file.</summary>
    /// <param name="path">Path of the file.</param>
    /// <returns>The parsed table.</returns>
    public static SkillTable Open(string path) => Parse(File.ReadAllBytes(path));

    /// <summary>Parses the contents of Skill.dat: 148-byte records after a header record; records with id 0 are skipped.</summary>
    /// <param name="data">The whole file.</param>
    /// <returns>The parsed table.</returns>
    public static SkillTable Parse(ReadOnlySpan<byte> data)
    {
        var byId = new Dictionary<ushort, SkillRecord>();
        for (var offset = RecordSize; offset + RecordSize <= data.Length; offset += RecordSize)
        {
            var r = data.Slice(offset, RecordSize);
            var id = U16(r, 22);
            if (id == 0)
                continue;
            var chars = r.Slice(1, 20).ToArray();
            Array.Reverse(chars);
            var name = EveText.Current.GetString(chars, 0, Math.Min((int)r[0], 20)).TrimEnd('\0');
            byId.TryAdd(id, new SkillRecord(id, name, U16(r, 24), U8(r, 29), U8(r, 99)) // 24 SpCost, 29 Type, 99 TargetType
            {
                Tree = U8(r, 26),
                Key = U16(r, 97),
                LearnCost = U8(r, 53),
                Effect = U8(r, 52),
                Duration = U8(r, 51),
                Power = U16(r, 49),
                PrereqSkillId = U16(r, 54),
                MaxLevel = U8(r, 105),
                Areas = [U8(r, 109), U8(r, 110), U8(r, 111), U8(r, 112)], // 109-112 the target area code of each level tier
            });
        }
        return new SkillTable(byId);
    }

    /// <summary>Decodes a byte field of a record.</summary>
    /// <param name="r">The record.</param>
    /// <param name="at">The offset of the field.</param>
    /// <returns>The value.</returns>
    private static byte U8(ReadOnlySpan<byte> r, int at) => (byte)((r[at] ^ 253) - 4);

    /// <summary>Decodes a little-endian u16 field of a record.</summary>
    /// <param name="r">The record.</param>
    /// <param name="at">The offset of the field.</param>
    /// <returns>The value.</returns>
    private static ushort U16(ReadOnlySpan<byte> r, int at) => (ushort)((BinaryPrimitives.ReadUInt16LittleEndian(r[at..]) ^ 28320) - 4);
}
