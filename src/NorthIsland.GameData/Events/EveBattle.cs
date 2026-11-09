namespace NorthIsland.GameData.Events;

/// <summary>A scripted battle placed on the map, with the members of its two sides.</summary>
public sealed record EveBattle : EvePlacedObject
{
    /// <summary>
    /// Byte read at offset 291 of the original record (name taken from that offset); its meaning is not established and nothing here uses it.
    /// </summary>
    public byte Unknown291 { get; init; }

    /// <summary>
    /// Word read at offset 292 of the original record (name taken from that offset); its meaning is not established and nothing here uses it.
    /// </summary>
    public ushort Unknown292 { get; init; }

    /// <summary>
    /// Byte read at offset 294 of the original record (name taken from that offset); its meaning is not established and nothing here uses it.
    /// </summary>
    public byte Unknown294 { get; init; }

    /// <summary>The members of the left side.</summary>
    public IReadOnlyList<EveBattleMember> Left { get; init; } = [];

    /// <summary>The members of the right side.</summary>
    public IReadOnlyList<EveBattleMember> Right { get; init; } = [];

    /// <summary>
    /// Byte read at offset 295 of the original record (name taken from that offset); its meaning is not established and nothing here uses it.
    /// </summary>
    public byte Unknown295 { get; init; }
}
