namespace NorthIsland.Server.Features.Items;

/// <summary>A gift card and what it can be exchanged for.</summary>
/// <param name="Id">The item id of the card.</param>
/// <param name="Whole">True when the choices are everything the card offers, false when they are only a sample.</param>
/// <param name="Choices">The items the card can be exchanged for.</param>
public sealed record GiftCard(
    ushort Id,
    bool Whole,
    IReadOnlyList<GiftCardChoice> Choices);
