namespace NorthIsland.GameData.Maps;

/// <summary>A rectangular battle zone of a map.</summary>
/// <param name="Id">The zone id.</param>
/// <param name="X1">X coordinate of one corner.</param>
/// <param name="Y1">Y coordinate of one corner.</param>
/// <param name="X2">X coordinate of the opposite corner.</param>
/// <param name="Y2">Y coordinate of the opposite corner.</param>
public readonly record struct MapZone(
    byte Id,
    int X1,
    int Y1,
    int X2,
    int Y2)
{
    /// <summary>Tells whether a point is inside the rectangle, borders included, whichever corner comes first.</summary>
    /// <param name="x">X coordinate.</param>
    /// <param name="y">Y coordinate.</param>
    /// <returns>True when the point is inside.</returns>
    public bool Contains(int x, int y) =>
        x >= Math.Min(X1, X2) && x <= Math.Max(X1, X2) && y >= Math.Min(Y1, Y2) && y <= Math.Max(Y1, Y2);
}
