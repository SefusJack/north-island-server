namespace NorthIsland.GameData.Events;

/// <summary>An NPC placed on the map, with its movement path and item records.</summary>
public sealed record EveNpc : EvePlacedObject
{
    /// <summary>The NPC template id.</summary>
    public int NpcId { get; init; }

    /// <summary>The facing of the NPC.</summary>
    public byte Direction { get; init; }

    /// <summary>The movement kind byte as stored.</summary>
    public byte MoveKind { get; init; }

    /// <summary>
    /// Byte read at offset 684 of the original record (name taken from that offset); its meaning is not established and nothing here uses it.
    /// </summary>
    public byte Unknown684 { get; init; }

    /// <summary>The movement path points, at most 50.</summary>
    public IReadOnlyList<EveNpcPathPoint> Path { get; init; } = [];

    /// <summary>The movement speed byte as stored.</summary>
    public byte Speed { get; init; }

    /// <summary>
    /// Byte read at offset 685 of the original record (name taken from that offset); its meaning is not established and nothing here uses it.
    /// </summary>
    public byte Unknown685 { get; init; }

    /// <summary>
    /// Int read at offset 688 of the original record (name taken from that offset); its meaning is not established and nothing here uses it.
    /// </summary>
    public int Unknown688 { get; init; }

    /// <summary>
    /// Int read at offset 692 of the original record (name taken from that offset); its meaning is not established and nothing here uses it.
    /// </summary>
    public int Unknown692 { get; init; }

    /// <summary>
    /// Word read at offset 696 of the original record (name taken from that offset); its meaning is not established and nothing here uses it.
    /// </summary>
    public ushort Unknown696 { get; init; }

    /// <summary>Flag byte read at offset 698 of the original record; its bits are not decoded here.</summary>
    public byte Unknown698Flags { get; init; }

    /// <summary>The 92-byte item sub-records of the NPC.</summary>
    public IReadOnlyList<EveNpcItem> Items { get; init; } = [];

    /// <summary>
    /// Word read at offset 1712 of the original record (name taken from that offset); its meaning is not established and nothing here uses it.
    /// </summary>
    public ushort Unknown1712 { get; init; }

    /// <summary>
    /// Word read at offset 1714 of the original record (name taken from that offset); its meaning is not established and nothing here uses it.
    /// </summary>
    public ushort Unknown1714 { get; init; }

    /// <summary>
    /// Int read at offset 1716 of the original record (name taken from that offset); its meaning is not established and nothing here uses it.
    /// </summary>
    public int Unknown1716 { get; init; }
}
