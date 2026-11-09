namespace NorthIsland.GameData.Events;

/// <summary>A door (exit zone) tile rectangle that runs its events when entered.</summary>
public sealed record EveDoor : EvePlacedObject
{
    /// <summary>The x of the opposite rectangle corner (the other corner is <c>X</c>).</summary>
    public int X2 { get; init; }

    /// <summary>The y of the opposite rectangle corner (the other corner is <c>Y</c>).</summary>
    public int Y2 { get; init; }

    /// <summary>The door kind byte as stored.</summary>
    public byte Kind { get; init; }

    /// <summary>The x offset value stored with the door.</summary>
    public int OffsetX { get; init; }

    /// <summary>The y offset value stored with the door.</summary>
    public int OffsetY { get; init; }

    /// <summary>The trigger rectangle in map pixels.</summary>
    public EvePixelRect Area => EvePixelRect.FromTiles(X, Y, X2, Y2);
}
