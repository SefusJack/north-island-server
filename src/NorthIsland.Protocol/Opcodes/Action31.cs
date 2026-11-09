namespace NorthIsland.Protocol.Opcodes;

/// <summary>Sub codes of action <c>31</c>: the inn, with resting and pet storage.</summary>
public static class Action31
{
    /// <summary>The action byte <c>31</c> that starts every packet of this group.</summary>
    public const byte Code = 31;

    /// <summary>Client to server: accepts the inn's price and rests.</summary>
    public const byte AcceptRest = 1;

    /// <summary>Server to client: answer to the rest request.</summary>
    public const byte RestAnswer = 1;

    /// <summary>Server to client: the price of a rest.</summary>
    public const byte RestPrice = 2;

    /// <summary>Server to client: the rest begins; no data.</summary>
    public const byte Sleep = 10;

    /// <summary>Client to server: takes stored pets out of the inn.</summary>
    public const byte Withdraw = 2;

    /// <summary>Both ways: stores pets in the inn (client request); the server confirms a stored pet with the storage index and pet slot.</summary>
    public const byte Store = 3;

    /// <summary>Both ways: swaps stored pets (client request); the server clears a storage index.</summary>
    public const byte Swap = 4;

    /// <summary>Pet storage sub code; nothing in the server sends or handles it.</summary>
    public const byte Retrieved = 5;

    /// <summary>Server to client: the stored pets, as records.</summary>
    public const byte StorageList = 6;

    /// <summary>Server to client: opens the pet storage form; no data.</summary>
    public const byte StorageOpen = 7;

    /// <summary>Server to client: confirms a retrieved pet with the storage index and pet slot.</summary>
    public const byte RetrievedOld = 11;

    /// <summary>Server to client: releases the client after a storage request.</summary>
    public const byte StorageDone = 12;

    /// <summary>Server to client: refuses a store because the storer already has that pet.</summary>
    public const byte StorerHasPet = 13;

    /// <summary>Server to client: refuses a store because the same pet is already stored.</summary>
    public const byte SamePet = 14;
}
