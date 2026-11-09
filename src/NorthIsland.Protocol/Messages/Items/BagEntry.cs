using NorthIsland.Protocol.IO;

namespace NorthIsland.Protocol.Messages.Items;

/// <summary>One item stack in the player's bag as sent in the 23/5 and 23/8 packets.</summary>
public sealed record BagEntry
{
    /// <summary>Bag slot of the stack.</summary>
    public required byte Slot { get; init; }

    /// <summary>Item id.</summary>
    public required ushort ItemId { get; init; }

    /// <summary>Number of items in the stack.</summary>
    public byte Count { get; init; } = 1;

    /// <summary>Wear value of the item.</summary>
    public byte Wear { get; init; } = 1;

    /// <summary>Byte at offset 5 of the record; always written as given, its meaning is not known.</summary>
    public byte Unknown5 { get; init; }

    /// <summary>Byte at offset 6 of the record; always written as given, its meaning is not known.</summary>
    public byte Unknown6 { get; init; }

    /// <summary>Amount of the item already used up.</summary>
    public int UsedAmount { get; init; }

    /// <summary>Extra amount stored with the item.</summary>
    public ushort ExtraAmount { get; init; }

    /// <summary>Id of the magic crystal stone set in the item, 0 for none.</summary>
    public ushort Insert { get; init; }

    /// <summary>Expiry time of the item as a double, 0 for none.</summary>
    public double Expiry { get; init; }

    /// <summary>Fusion level of the item.</summary>
    public byte FusionLevel { get; init; }

    /// <summary>Byte at offset 24 of the record; always written as given, its meaning is not known.</summary>
    public byte Unknown24 { get; init; }

    /// <summary>Item id of the pose look applied to the item, 0 for none.</summary>
    public ushort PoseItemId { get; init; }

    /// <summary>Item id of the box the item came in, 0 for none.</summary>
    public ushort BoxItemId { get; init; }

    /// <summary>Appends the 29-byte record to a packet.</summary>
    /// <param name="w">The packet being built.</param>
    public void WriteTo(PacketWriter w) =>
        w.U8(Slot).U16(ItemId).U8(Count).U8(Wear).U8(Unknown5).U8(Unknown6).I32(UsedAmount).U16(ExtraAmount).U16(Insert)
            .F64(Expiry).U8(FusionLevel).U8(Unknown24).U16(PoseItemId).U16(BoxItemId);
}
