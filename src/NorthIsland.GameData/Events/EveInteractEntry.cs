namespace NorthIsland.GameData.Events;

/// <summary>One entry of an interaction record.</summary>
/// <param name="Unknown0">Byte at offset 0; its meaning is not established.</param>
/// <param name="Value">The word value of the entry.</param>
/// <param name="Unknown3">Byte at offset 3; its meaning is not established.</param>
/// <param name="Kind">The kind byte of the entry.</param>
public readonly record struct EveInteractEntry(
    byte Unknown0,
    ushort Value,
    byte Unknown3,
    byte Kind);
