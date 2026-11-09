namespace NorthIsland.Protocol.Opcodes;

/// <summary>Action 15: vehicles, pets and the constellation.</summary>
public static class Action15
{
    /// <summary>Action code 15.</summary>
    public const byte Code = 15;

    /// <summary>Client to server: constellation request.</summary>
    public const byte Constellation = 15;

    /// <summary>Client to server: board a vehicle.</summary>
    public const byte Board = 7;

    /// <summary>Client to server: leave a vehicle.</summary>
    public const byte Park = 8;

    /// <summary>Client to server: place a vehicle from the bag.</summary>
    public const byte PlaceFromBag = 9;

    /// <summary>Client to server: the player clicked their own vehicle.</summary>
    public const byte ClickVehicle = 10;

    /// <summary>Client to server: the wreck animation of a vehicle has finished.</summary>
    public const byte WreckSeen = 13;

    /// <summary>Client to server: the player walked into water with a vehicle item; the server places it and boards.</summary>
    public const byte WalkIntoWater = 14;

    /// <summary>Server to client: a player boarded a vehicle.</summary>
    public const byte Boarded = 10;

    /// <summary>Server to client: a player left a vehicle.</summary>
    public const byte Dismounted = 11;

    /// <summary>Server to client: a vehicle was placed.</summary>
    public const byte Placed = 12;

    /// <summary>Server to client: the placed vehicle went back to the bag.</summary>
    public const byte TakenBack = 13;

    /// <summary>Server to client: a vehicle was placed and the owner boards.</summary>
    public const byte PlacedAndBoard = 18;

    /// <summary>Server to client: a vehicle was destroyed.</summary>
    public const byte Destroyed = 15;

    /// <summary>Server to client: wear and fuel of the riding owner's vehicle.</summary>
    public const byte Wear = 14;

    /// <summary>Server to client: the vehicle has no fuel.</summary>
    public const byte NoFuel = 23;

    /// <summary>Server to client: a vehicle leaves the scene.</summary>
    public const byte Removed = 22;

    /// <summary>Server to client: a pet joined a team.</summary>
    public const byte PetJoined = 1;

    /// <summary>Server to client: a pet left a team.</summary>
    public const byte PetRemoved = 2;

    /// <summary>Client to server: exile a pet.</summary>
    public const byte ExilePet = 2;

    /// <summary>Client to server: rename a pet.</summary>
    public const byte RenamePet = 6;

    /// <summary>Server to client: the active pets of other players in the scene.</summary>
    public const byte ScenePets = 4;

    /// <summary>Server to client: the pet list records.</summary>
    public const byte PetList = 8;

    /// <summary>Client to server: mount a pet.</summary>
    public const byte RidePet = 11;

    /// <summary>Client to server: dismount a pet.</summary>
    public const byte LeavePet = 12;

    /// <summary>Server to client: a player mounted a pet.</summary>
    public const byte PetMounted = 16;

    /// <summary>Server to client: the gear of a team pet went into the warehouse.</summary>
    public const byte PetGearStored = 21;

    /// <summary>Server to client: a player dismounted a pet.</summary>
    public const byte PetDismounted = 17;

    /// <summary>Server to client: a pet was renamed.</summary>
    public const byte PetRenamed = 9;
}
