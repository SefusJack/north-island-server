namespace NorthIsland.Protocol.Messages.Items;

/// <summary>Result code of a sale to a shop, sent in <c>27/2</c>.</summary>
public enum SellResult : byte
{
    /// <summary>Result 0: the sale went through.</summary>
    Ok = 0,

    /// <summary>Result 1: named for a full account; no handler sends it.</summary>
    AccountFull = 1,

    /// <summary>Result 2: the item is not in the player's inventory.</summary>
    NotInInventory = 2,

    /// <summary>Result 3: named for too many items; no handler sends it.</summary>
    TooMany = 3,

    /// <summary>Result 4: the sale failed.</summary>
    Failed = 4,
}
