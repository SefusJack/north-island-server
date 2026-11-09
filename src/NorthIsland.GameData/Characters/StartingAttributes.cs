namespace NorthIsland.GameData.Characters;

/// <summary>The five base attributes of a newly created character.</summary>
/// <param name="Strength">Strength.</param>
/// <param name="Constitution">Constitution.</param>
/// <param name="Intelligence">Intelligence.</param>
/// <param name="Wisdom">Wisdom.</param>
/// <param name="Agility">Agility.</param>
public readonly record struct StartingAttributes(
    ushort Strength,
    ushort Constitution,
    ushort Intelligence,
    ushort Wisdom,
    ushort Agility);
