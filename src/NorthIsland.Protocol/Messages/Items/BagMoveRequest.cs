using NorthIsland.Protocol.IO;

namespace NorthIsland.Protocol.Messages.Items;

/// <summary>The client's request to move items between bag slots (23/10).</summary>
/// <param name="From">Source slot.</param>
/// <param name="Count">Number of items to move.</param>
/// <param name="To">Target slot.</param>
public sealed record BagMoveRequest(
    byte From,
    byte Count,
    byte To)
{
    /// <summary>Reads the request from a packet body.</summary>
    /// <param name="body">The packet body after the action and sub code.</param>
    /// <returns>The parsed request.</returns>
    public static BagMoveRequest Parse(ReadOnlySpan<byte> body)
    {
        var r = new PacketReader(body);
        return new BagMoveRequest(r.U8(), r.U8(), r.U8());
    }
}
