namespace NorthIsland.Server.World;

/// <summary>Where a map change puts a player: the map and the spot on it, with the door and house room details sent along.</summary>
/// <param name="MapId">The target map id.</param>
/// <param name="X">The target x position.</param>
/// <param name="Y">The target y position.</param>
/// <param name="ConveyIndex">The index of the door (convey) used, 0 when not through a door.</param>
/// <param name="RoomSize">The house room size sent with the map change, 0 for none.</param>
/// <param name="HouseRoom">The house room number of the target, 0 for none.</param>
public sealed record DoorWarp(
    ushort MapId,
    ushort X,
    ushort Y,
    ushort ConveyIndex,
    byte RoomSize = 0,
    byte HouseRoom = 0);
