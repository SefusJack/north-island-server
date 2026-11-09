namespace NorthIsland.Server.Features.Items;

/// <summary>One item a gift card can be exchanged for.</summary>
/// <param name="ItemId">The item id.</param>
/// <param name="Count">How many of the item one card gives.</param>
public sealed record GiftCardChoice(
    ushort ItemId,
    byte Count);
