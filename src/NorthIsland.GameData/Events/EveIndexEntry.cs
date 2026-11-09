namespace NorthIsland.GameData.Events;

/// <summary>One entry of the scene index of the archive.</summary>
/// <param name="MapId">The map the scene is for.</param>
/// <param name="SourceMapId">The source map id stored in the entry.</param>
/// <param name="Offset">Byte offset of the scene blob in the file.</param>
/// <param name="Size">Size in bytes of the scene blob.</param>
public readonly record struct EveIndexEntry(
    ushort MapId,
    ushort SourceMapId,
    int Offset,
    int Size);
