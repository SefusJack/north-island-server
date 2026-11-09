namespace NorthIsland.Protocol.Messages.Items;

/// <summary>One item offered in a player's item stall.</summary>
/// <param name="BagSlot">Bag slot of the stack in the owner's bag.</param>
/// <param name="ItemId">Item id.</param>
/// <param name="Count">Number of items in the stack.</param>
/// <param name="Wear">Wear value of the item.</param>
/// <param name="Price">Unit price in gold.</param>
public sealed record StallItem(
    byte BagSlot,
    ushort ItemId,
    byte Count,
    byte Wear,
    int Price);
