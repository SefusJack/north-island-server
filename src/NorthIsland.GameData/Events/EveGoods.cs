namespace NorthIsland.GameData.Events;

/// <summary>A ground object placed on the map.</summary>
public sealed record EveGoods : EvePlacedObject
{
    /// <summary>
    /// Int read at offset 64 of the original record (name taken from that offset); its meaning is not established and nothing here uses it.
    /// </summary>
    public int Unknown64 { get; init; }

    /// <summary>
    /// Word read at offset 68 of the original record (name taken from that offset); its meaning is not established and nothing here uses it.
    /// </summary>
    public ushort Unknown68 { get; init; }

    /// <summary>
    /// Byte read at offset 70 of the original record (name taken from that offset); its meaning is not established and nothing here uses it.
    /// </summary>
    public byte Unknown70 { get; init; }

    /// <summary>
    /// Int read at offset 72 of the original record (name taken from that offset); its meaning is not established and nothing here uses it.
    /// </summary>
    public int Unknown72 { get; init; }
}
