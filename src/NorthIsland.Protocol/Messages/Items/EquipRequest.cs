using NorthIsland.Protocol.IO;

namespace NorthIsland.Protocol.Messages.Items;

/// <summary>The client's request to put on an item from the bag.</summary>
/// <param name="BagSlot">Bag slot of the item.</param>
public sealed record EquipRequest(
    byte BagSlot)
{
    /// <summary>Reads the request from a packet body.</summary>
    /// <param name="body">The packet body after the action and sub code.</param>
    /// <returns>The parsed request.</returns>
    public static EquipRequest Parse(ReadOnlySpan<byte> body) => new(new PacketReader(body).U8());
}
