namespace NorthIsland.Protocol.Messages.Items;

/// <summary>One item mall row.</summary>
/// <param name="ItemId">Item id.</param>
/// <param name="Pack">How many items one purchase gives.</param>
/// <param name="Price">Price in mall points.</param>
/// <param name="Discount">Discount percentage factor; the price paid is price x discount / 100.</param>
/// <param name="Hot">Whether the row is on the first tab.</param>
/// <param name="Category">Category, which decides the tab.</param>
/// <param name="Key">Row key for the 2014 client, 0 when none.</param>
public sealed record MallRow(
    ushort ItemId,
    byte Pack,
    ushort Price,
    byte Discount,
    byte Hot,
    byte Category,
    ushort Key = 0);
