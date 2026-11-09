namespace NorthIsland.GameData.Events;

/// <summary>A rectangle in map pixels.</summary>
/// <param name="Left">The left edge.</param>
/// <param name="Top">The top edge.</param>
/// <param name="Width">The width.</param>
/// <param name="Height">The height.</param>
public readonly record struct EvePixelRect(
    int Left,
    int Top,
    int Width,
    int Height)
{
    /// <summary>Converts a 1-based tile rectangle (20 pixels per tile) to pixels; the corners may be given in either order.</summary>
    /// <param name="x1">The x of the first corner in tiles.</param>
    /// <param name="y1">The y of the first corner in tiles.</param>
    /// <param name="x2">The x of the opposite corner in tiles.</param>
    /// <param name="y2">The y of the opposite corner in tiles.</param>
    /// <returns>The pixel rectangle.</returns>
    public static EvePixelRect FromTiles(int x1, int y1, int x2, int y2) =>
        new((x1 - 1) * 20, (y1 - 1) * 20, (Math.Abs(x2 - x1) + 1) * 20, (Math.Abs(y2 - y1) + 1) * 20);

    /// <summary>Tells whether a pixel position lies inside the rectangle (right and bottom edges excluded).</summary>
    /// <param name="x">The x in pixels.</param>
    /// <param name="y">The y in pixels.</param>
    /// <returns>True when the position is inside.</returns>
    public bool Contains(int x, int y) => x >= Left && x < Left + Width && y >= Top && y < Top + Height;
}
