namespace NorthIsland.GameData.Events;

/// <summary>Reads one section record from a scene reader.</summary>
/// <typeparam name="T">The record type.</typeparam>
/// <param name="reader">The reader positioned after the record index.</param>
/// <param name="index">The record index read from the section.</param>
/// <returns>The decoded record.</returns>
public delegate T EveRecordReader<out T>(ref EveReader reader, int index);
