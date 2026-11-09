using NorthIsland.Protocol.IO;

namespace NorthIsland.Protocol.Messages.World;

/// <summary>A vehicle request from the client: which bag item it concerns.</summary>
/// <param name="BagSlot">Bag slot of the vehicle item.</param>
/// <param name="ItemId">Item id the client expects in that slot.</param>
public readonly record struct VehicleRequest(
    byte BagSlot,
    ushort ItemId)
{
    /// <summary>Reads a vehicle request from a packet body.</summary>
    /// <param name="body">The packet body after the action and sub code.</param>
    /// <returns>The request.</returns>
    public static VehicleRequest Parse(ReadOnlySpan<byte> body)
    {
        var r = new PacketReader(body);
        return new VehicleRequest(r.U8(), r.U16());
    }
}
