namespace NorthIsland.Protocol.Opcodes;

/// <summary>Action id 80: the point store.</summary>
public static class Action80
{
    /// <summary>Action id of this packet group, 80.</summary>
    public const byte Code = 80;

    /// <summary>Client to server: buys a point store item (same value as <c>List</c>).</summary>
    public const byte Buy = 1;

    /// <summary>Server to client: the point store list (same value as <c>Buy</c>).</summary>
    public const byte List = 1;

    /// <summary>Server to client: a purchase succeeded (same value as <c>BuyPetLevel</c>).</summary>
    public const byte Bought = 3;

    /// <summary>Server to client: the player has not enough points.</summary>
    public const byte NotEnough = 7;

    /// <summary>Client to server: spends points on a player level.</summary>
    public const byte BuyLevel = 2;

    /// <summary>Client to server: spends points on a pet level (same value as <c>Bought</c>).</summary>
    public const byte BuyPetLevel = 3;

    /// <summary>Server to client: the server is busy.</summary>
    public const byte Busy = 8;
}
