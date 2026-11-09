using NorthIsland.Protocol.IO;

namespace NorthIsland.Protocol.Messages.Items;

/// <summary>A request from the client to take off a worn item into the bag.</summary>
/// <param name="EquipSlot">The equipment slot to empty.</param>
/// <param name="BagSlot">The bag slot the item goes to.</param>
public sealed record UnequipRequest(
    byte EquipSlot,
    byte BagSlot)
{
    /// <summary>Parses the request.</summary>
    /// <param name="body">The packet body.</param>
    /// <returns>The parsed request.</returns>
    public static UnequipRequest Parse(ReadOnlySpan<byte> body)
    {
        var r = new PacketReader(body);
        return new UnequipRequest(r.U8(), r.U8());
    }
}
