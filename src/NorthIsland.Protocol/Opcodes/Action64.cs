namespace NorthIsland.Protocol.Opcodes;

/// <summary>Action id 64: manufacturing in a house.</summary>
public static class Action64
{
    /// <summary>Action id of this packet group, 64.</summary>
    public const byte Code = 64;

    /// <summary>Client to server: the manufacture order (same value as <c>Jobs</c>).</summary>
    public const byte Make = 1;

    /// <summary>Server to client: the manufacture jobs of the player (same value as <c>Make</c>).</summary>
    public const byte Jobs = 1;

    /// <summary>Client to server: continues a manufacture job.</summary>
    public const byte Start = 2;

    /// <summary>Client to server: stops a manufacture job; server to client: a job is stopped.</summary>
    public const byte Stop = 3;

    /// <summary>Server to client: a manufacture job is finished (same value as <c>GiveUp</c>).</summary>
    public const byte Finish = 4;

    /// <summary>Client to server: gives up a manufacture job (same value as <c>Finish</c>).</summary>
    public const byte GiveUp = 4;

    /// <summary>Server to client: a manufacture job was given up.</summary>
    public const byte GivenUp = 5;

    /// <summary>Server to client: a manufacture job ended because the bag is full.</summary>
    public const byte BagFull = 6;

    /// <summary>Server to client: the fuel of a gas storage and whether the job is finished.</summary>
    public const byte GasState = 12;

    /// <summary>Server to client: the big gas storage refused the deposit.</summary>
    public const byte GasRefused = 13;

    /// <summary>Server to client: the small gas storage refused the deposit.</summary>
    public const byte SmallGasRefused = 15;

    /// <summary>Server to client: the pet leaves the work and the queue is reset.</summary>
    public const byte PetLeft = 9;

    /// <summary>Server to client: a pet's order was not taken.</summary>
    public const byte PetRefused = 11;

    /// <summary>Server to client: the pet that does the job.</summary>
    public const byte PetMaker = 10;
}
