namespace NorthIsland.Protocol.Messages.World;

/// <summary>A pitched tent as the camp packets describe it.</summary>
/// <param name="OwnerId">Id of the player who owns the tent.</param>
/// <param name="ItemId">Item id of the tent.</param>
/// <param name="X">X position in pixels.</param>
/// <param name="Y">Y position in pixels.</param>
/// <param name="Kind1">First tent style byte.</param>
/// <param name="Kind2">Second tent style byte.</param>
public readonly record struct CampRecord(
    int OwnerId,
    ushort ItemId,
    int X,
    int Y,
    byte Kind1 = 0,
    byte Kind2 = 0);
