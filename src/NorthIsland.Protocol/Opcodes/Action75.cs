namespace NorthIsland.Protocol.Opcodes;

/// <summary>Action id 75: the item mall.</summary>
public static class Action75
{
    /// <summary>Action id of this packet group, 75.</summary>
    public const byte Code = 75;

    /// <summary>Client to server: buys a mall item (same value as <c>Catalog</c>).</summary>
    public const byte Buy = 1;

    /// <summary>Client to server: buys a line of the second list with bonus points.</summary>
    public const byte BonusBuy = 5;

    /// <summary>Server to client: the player's bonus points.</summary>
    public const byte Bonus = 8;

    /// <summary>Server to client: bonus points left and spent after a purchase.</summary>
    public const byte BonusDeducted = 9;

    /// <summary>Server to client: the second mall list.</summary>
    public const byte CatalogB = 10;

    /// <summary>Server to client: the mall catalog (same value as <c>Buy</c>).</summary>
    public const byte Catalog = 1;

    /// <summary>Server to client: points left and spent after a purchase (same value as <c>Forge</c>).</summary>
    public const byte Deducted = 3;

    /// <summary>Server to client: a purchase succeeded (same value as <c>PlayGame</c>).</summary>
    public const byte Bought = 4;

    /// <summary>Server to client: a purchase was refused, with a reason code (same value as <c>BonusBuy</c>).</summary>
    public const byte Refused = 5;

    /// <summary>Client to server: confirms the forge tab for a bag item (same value as <c>Deducted</c>).</summary>
    public const byte Forge = 3;

    /// <summary>Client to server: starts a game from the mall game tab (same value as <c>Bought</c>).</summary>
    public const byte PlayGame = 4;

    /// <summary>Server to client: opens or closes the game tab.</summary>
    public const byte GameTab = 7;

    /// <summary>Server to client: the result of a forge.</summary>
    public const byte ForgeResult = 6;
}
