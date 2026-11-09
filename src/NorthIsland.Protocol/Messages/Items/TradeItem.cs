namespace NorthIsland.Protocol.Messages.Items;

/// <summary>One item of a trade offer as the partner sees it.</summary>
/// <param name="ItemId">Item id.</param>
/// <param name="Count">Number of items in the stack.</param>
/// <param name="Wear">Wear value of the item.</param>
public readonly record struct TradeItem(
    ushort ItemId,
    byte Count,
    byte Wear);
