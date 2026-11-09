namespace NorthIsland.GameData.Events;

/// <summary>An event: a named list of conditions, each with results.</summary>
public sealed record EveEvent
{
    /// <summary>The event index within its section.</summary>
    public int Index { get; init; }

    /// <summary>The flag bits stored with the event.</summary>
    public byte Flags { get; init; }

    /// <summary>True when flag bit 3 (value 8) is set.</summary>
    public bool IsFieldMonster => (Flags & 8) != 0;

    /// <summary>The event name.</summary>
    public string Name { get; init; } = "";

    /// <summary>The conditions with their results, in file order.</summary>
    public IReadOnlyList<EveCondition> Conditions { get; init; } = [];
}
