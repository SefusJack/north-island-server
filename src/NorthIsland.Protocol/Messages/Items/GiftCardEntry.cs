namespace NorthIsland.Protocol.Messages.Items;

/// <summary>One exchange card with the items it can be exchanged for.</summary>
/// <param name="Id">Id of the card.</param>
/// <param name="Version">Version of the card's list.</param>
/// <param name="Choices">Item id and amount of each choice.</param>
public sealed record GiftCardEntry(
    ushort Id,
    byte Version,
    IReadOnlyList<(ushort Item, byte Amount)> Choices);
