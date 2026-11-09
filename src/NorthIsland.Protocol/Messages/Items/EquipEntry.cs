using NorthIsland.Protocol.IO;

namespace NorthIsland.Protocol.Messages.Items;

/// <summary>One worn item as sent in the 23/11 equipment list.</summary>
public sealed record EquipEntry
{
    /// <summary>Item id.</summary>
    public required ushort ItemId { get; init; }

    /// <summary>Wear value of the item.</summary>
    public byte Wear { get; init; } = 1;

    /// <summary>Byte at offset 3 of the record; always written as given, its meaning is not known.</summary>
    public byte Unknown3 { get; init; }

    /// <summary>Byte at offset 4 of the record; always written as given, its meaning is not known.</summary>
    public byte Unknown4 { get; init; }

    /// <summary>Id of the magic crystal stone set in the item, 0 for none.</summary>
    public ushort Insert { get; init; }

    /// <summary>Expiry time of the item as a double, 0 for none.</summary>
    public double Expiry { get; init; }

    /// <summary>Fusion level of the item.</summary>
    public byte FusionLevel { get; init; }

    /// <summary>Attribute adjustment byte of the item.</summary>
    public byte AttrAdjust { get; init; }

    /// <summary>Item id of the look shown instead of the item, 0 for none.</summary>
    public ushort LookItemId { get; init; }

    /// <summary>Appends the 19-byte record to a packet.</summary>
    /// <param name="w">The packet being built.</param>
    public void WriteTo(PacketWriter w) =>
        w.U16(ItemId).U8(Wear).U8(Unknown3).U8(Unknown4).U16(Insert).F64(Expiry).U8(FusionLevel).U8(AttrAdjust).U16(LookItemId);
}
