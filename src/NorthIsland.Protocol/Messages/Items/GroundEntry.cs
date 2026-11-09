namespace NorthIsland.Protocol.Messages.Items;

/// <summary>One item or gold pile lying on the ground of a map.</summary>
/// <param name="Index">Index of the entry on the map.</param>
/// <param name="ItemId">Item id, unused for gold.</param>
/// <param name="X">X position.</param>
/// <param name="Y">Y position.</param>
/// <param name="Gold">Amount of gold; above 0 the entry is a gold pile.</param>
public readonly record struct GroundEntry(
    ushort Index,
    ushort ItemId,
    ushort X,
    ushort Y,
    int Gold = 0);
