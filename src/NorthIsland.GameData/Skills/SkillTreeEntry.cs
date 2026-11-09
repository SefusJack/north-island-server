namespace NorthIsland.GameData.Skills;

/// <summary>One cell of an element skill tree.</summary>
/// <param name="SkillId">The skill of the cell; 0 for an empty cell.</param>
/// <param name="Strength">The strength needed.</param>
/// <param name="Constitution">The constitution needed.</param>
/// <param name="Intelligence">The intelligence needed.</param>
/// <param name="Wisdom">The wisdom needed.</param>
/// <param name="Agility">The agility needed.</param>
public sealed record SkillTreeEntry(
    ushort SkillId,
    ushort Strength,
    ushort Constitution,
    ushort Intelligence,
    ushort Wisdom,
    ushort Agility)
{
    /// <summary>Whether the stats meet the cell's requirements.</summary>
    /// <param name="strength">The strength.</param>
    /// <param name="constitution">The constitution.</param>
    /// <param name="intelligence">The intelligence.</param>
    /// <param name="wisdom">The wisdom.</param>
    /// <param name="agility">The agility.</param>
    /// <returns>True when every stat is at least the required value.</returns>
    public bool IsMetBy(
        int strength,
        int constitution,
        int intelligence,
        int wisdom,
        int agility) =>
            strength >= Strength && constitution >= Constitution && intelligence >= Intelligence && wisdom >= Wisdom && agility >= Agility;
}
