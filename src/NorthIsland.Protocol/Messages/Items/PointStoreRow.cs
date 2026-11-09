namespace NorthIsland.Protocol.Messages.Items;

/// <summary>One item of the point store.</summary>
/// <param name="ItemId">Item id.</param>
/// <param name="Price">Price in points.</param>
/// <param name="Count">Number of items one purchase gives.</param>
public sealed record PointStoreRow(
    ushort ItemId,
    int Price,
    byte Count);
