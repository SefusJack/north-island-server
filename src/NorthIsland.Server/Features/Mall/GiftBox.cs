namespace NorthIsland.Server.Features.Mall;

/// <summary>The fixed contents of a mall gift box.</summary>
/// <param name="Items">Item id and count of each stack given when the box is opened.</param>
/// <param name="Gold">Gold given when the box is opened; 0 for none.</param>
public sealed record GiftBox(
    IReadOnlyList<(ushort Item, int Count)> Items,
    int Gold);
