namespace NorthIsland.Protocol.Messages.World;

/// <summary>One piece of furniture in a house as the 57-byte furniture record holds it.</summary>
/// <param name="ItemId">Item id of the furniture.</param>
/// <param name="X">Cell x.</param>
/// <param name="Y">Cell y.</param>
/// <param name="Z">Level (floor).</param>
/// <param name="Dir">Facing direction.</param>
/// <param name="Parent">Furniture index of the piece this one is attached to (0 for none).</param>
/// <param name="Children">Furniture indexes attached to this piece; at most 10 are written.</param>
/// <param name="Room">Room number.</param>
/// <param name="Retained">True when the piece is flagged as retained.</param>
public readonly record struct FurnitureRecord(
    ushort ItemId,
    int X,
    int Y,
    int Z,
    byte Dir,
    ushort Parent,
    IReadOnlyList<ushort> Children,
    byte Room,
    bool Retained = false);
