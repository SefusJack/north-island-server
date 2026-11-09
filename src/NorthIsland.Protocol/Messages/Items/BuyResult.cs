namespace NorthIsland.Protocol.Messages.Items;

/// <summary>Outcome of a purchase, sent to the client as one byte.</summary>
public enum BuyResult : byte
{
    /// <summary>The purchase went through.</summary>
    Ok = 0,

    /// <summary>The player does not have enough money.</summary>
    NotEnoughMoney = 1,

    /// <summary>The bag has no room for the item.</summary>
    InventoryFull = 2,

    /// <summary>Part of the cart was bought, but not everything fits in the bag.</summary>
    NotAllFit = 3,

    /// <summary>The purchase was refused, for example because no store is open or a row does not exist.</summary>
    Failed = 4,
}
