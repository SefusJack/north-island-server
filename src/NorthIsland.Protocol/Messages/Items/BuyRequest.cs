using NorthIsland.Protocol.IO;

namespace NorthIsland.Protocol.Messages.Items;

/// <summary>The client's shopping cart for an NPC store (27/1).</summary>
/// <param name="Lines">Store row (1-based) and quantity of each line.</param>
public sealed record BuyRequest(
    IReadOnlyList<(byte Row, byte Count)> Lines)
{
    /// <summary>Reads the cart from a packet body; lines the body is too short for are dropped.</summary>
    /// <param name="body">The packet body after the action and sub code.</param>
    /// <returns>The parsed request.</returns>
    public static BuyRequest Parse(ReadOnlySpan<byte> body)
    {
        var r = new PacketReader(body);
        var count = r.U8();
        var lines = new List<(byte, byte)>(count);
        for (var i = 0; i < count && r.Remaining >= 2; i++)
            lines.Add((r.U8(), r.U8()));
        return new BuyRequest(lines);
    }
}
