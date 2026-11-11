namespace NorthIsland.GameData.Skills;

/// <summary>One Skill.dat record: what the server needs to know about a skill.</summary>
/// <param name="Id">The skill id.</param>
/// <param name="Name">The skill's name.</param>
/// <param name="SpCost">The SP the skill costs.</param>
/// <param name="Type">The skill type; 12 is a flee skill.</param>
/// <param name="TargetType">The target type; 1, 3 and 5 are self and ally targets.</param>
public sealed record SkillRecord(
    ushort Id,
    string Name,
    ushort SpCost,
    byte Type,
    byte TargetType)
{
    /// <summary>The element skill tree the skill belongs to: 1-4 for the player's element, 0 for none.</summary>
    public byte Tree { get; init; }

    /// <summary>The index 1..500 of the skill in the player's skill level and exp table.</summary>
    public ushort Key { get; init; }

    /// <summary>The status the skill puts on or takes off; 0 for none.</summary>
    public byte Effect { get; init; }

    /// <summary>The duration of the status the skill causes.</summary>
    public byte Duration { get; init; }

    /// <summary>The power of the skill.</summary>
    public ushort Power { get; init; }

    /// <summary>The skill points needed to learn the skill.</summary>
    public byte LearnCost { get; init; }

    /// <summary>The highest level the skill reaches.</summary>
    public byte MaxLevel { get; init; }

    /// <summary>The id of the skill that has to be learned first; 0 for none.</summary>
    public ushort PrereqSkillId { get; init; }

    /// <summary>The target area code of each of the four level tiers; 0 or 1 is the chosen fighter alone.</summary>
    public IReadOnlyList<byte> Areas { get; init; } = [];

    /// <summary>The target area code at a skill level: levels 1-3, 4-6 and 7-9 use the first three tiers, the maximum level the fourth.</summary>
    /// <param name="level">The skill level.</param>
    /// <returns>The area code, 1 when the record has none.</returns>
    public byte AreaAt(int level) =>
        Areas.Count < 4 ? (byte)1 : Math.Max((byte)1, Areas[level >= MaxLevel ? 3 : Math.Clamp((level - 1) / 3, 0, 2)]);
}
