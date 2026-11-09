namespace NorthIsland.Protocol.Opcodes;

/// <summary>Action id 83: refusal texts for pets and tents.</summary>
public static class Action83
{
    /// <summary>Action id of this packet group, 83.</summary>
    public const byte Code = 83;

    /// <summary>Server to client: the player's pet slots are full.</summary>
    public const byte PetSlotsFull = 1;

    /// <summary>Server to client: the player already has the same pet.</summary>
    public const byte SamePet = 2;

    /// <summary>Sub code 3 for a tent that already has a pet; no code uses it.</summary>
    public const byte TentHasPet = 3;

    /// <summary>Server to client: the pet keeper already has the same pet.</summary>
    public const byte KeeperHasPet = 4;
}
