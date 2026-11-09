namespace NorthIsland.GameData.Events;

/// <summary>An interaction (surface) record: a named list of entries.</summary>
public sealed record EveInteract
{
    /// <summary>The record index within its section.</summary>
    public int Index { get; init; }

    /// <summary>The record name.</summary>
    public string Name { get; init; } = "";

    /// <summary>The kind byte as stored.</summary>
    public byte Kind { get; init; }

    /// <summary>Word read after the kind (named after its offset 6); its meaning is not established.</summary>
    public ushort Unknown6 { get; init; }

    /// <summary>The entries of the record.</summary>
    public IReadOnlyList<EveInteractEntry> Entries { get; init; } = [];
}
