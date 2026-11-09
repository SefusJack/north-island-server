namespace NorthIsland.Protocol.Messages.Items;

/// <summary>One item in the Lost Property Office list.</summary>
/// <param name="ItemId">Item id.</param>
/// <param name="Insert">Id of the magic crystal stone set in the item, 0 for none.</param>
/// <param name="Fusion">Fusion level of the item.</param>
/// <param name="Pose">Item id of the pose look applied to the item, 0 for none.</param>
/// <param name="Key">Key sent back by the client to reclaim the item.</param>
public sealed record LostPropertyRecord(
    ushort ItemId,
    ushort Insert,
    byte Fusion,
    ushort Pose,
    ushort Key);
