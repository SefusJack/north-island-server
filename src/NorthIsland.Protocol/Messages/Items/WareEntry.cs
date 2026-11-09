namespace NorthIsland.Protocol.Messages.Items;

/// <summary>One stack in the warehouse list.</summary>
/// <param name="Slot">Warehouse slot.</param>
/// <param name="ItemId">Item id.</param>
/// <param name="Count">Number of items in the stack.</param>
/// <param name="Wear">Wear value of the item.</param>
public sealed record WareEntry(
    byte Slot,
    ushort ItemId,
    byte Count,
    byte Wear);
