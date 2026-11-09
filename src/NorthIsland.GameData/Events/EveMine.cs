namespace NorthIsland.GameData.Events;

/// <summary>A mining or gathering tile rectangle.</summary>
public sealed record EveMine : EvePlacedObject
{
    /// <summary>The x of the opposite rectangle corner (the other corner is <c>X</c>).</summary>
    public int X2 { get; init; }

    /// <summary>The y of the opposite rectangle corner (the other corner is <c>Y</c>).</summary>
    public int Y2 { get; init; }

    /// <summary>The mine kind byte as stored.</summary>
    public byte Kind { get; init; }

    /// <summary>
    /// Byte read at offset 73 of the original record (name taken from that offset); its meaning is not established and nothing here uses it.
    /// </summary>
    public byte Unknown73 { get; init; }

    /// <summary>The rectangle in map pixels.</summary>
    public EvePixelRect Area => EvePixelRect.FromTiles(X, Y, X2, Y2);
}
