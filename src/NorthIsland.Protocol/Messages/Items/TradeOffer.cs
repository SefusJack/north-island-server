using NorthIsland.Protocol.IO;

namespace NorthIsland.Protocol.Messages.Items;

/// <summary>The player's own offer from the client: gold and the bag slots of the items.</summary>
/// <param name="Gold">Gold offered.</param>
/// <param name="BagSlots">Bag slots of the offered items, one byte each.</param>
public sealed record TradeOffer(
    int Gold,
    IReadOnlyList<byte> BagSlots)
{
    /// <summary>Parses an offer. The first four bytes are the gold, every following byte is a bag slot.</summary>
    /// <param name="body">The packet body.</param>
    /// <returns>The parsed offer.</returns>
    public static TradeOffer Parse(ReadOnlySpan<byte> body)
    {
        var r = new PacketReader(body);
        var gold = r.I32();
        return new TradeOffer(gold, body[4..].ToArray());
    }
}
