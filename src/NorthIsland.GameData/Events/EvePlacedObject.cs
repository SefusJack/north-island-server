namespace NorthIsland.GameData.Events;

/// <summary>The common start of NPC, door, mine, goods and battle records: name, flags, position and event id lists.</summary>
public abstract record EvePlacedObject
{
    /// <summary>The record index within its section.</summary>
    public int Index { get; init; }

    /// <summary>The object name.</summary>
    public string Name { get; init; } = "";

    /// <summary>The flag bits stored with the object.</summary>
    public byte Flags { get; init; }

    /// <summary>The x position.</summary>
    public int X { get; init; }

    /// <summary>The y position.</summary>
    public int Y { get; init; }

    /// <summary>The indices of the events the object runs, at most 10.</summary>
    public IReadOnlyList<byte> EventIds { get; init; } = [];

    /// <summary>Second byte list (at most 10) read at offset 48; its meaning is not established.</summary>
    public IReadOnlyList<byte> Unknown48List { get; init; } = [];

    /// <summary>
    /// Byte read at offset 60 of the original record (name taken from that offset); its meaning is not established and nothing here uses it.
    /// </summary>
    public byte Unknown60 { get; init; }
}
