namespace NorthIsland.Protocol.Messages.Items;

/// <summary>The client's request to sell bag items to an NPC (27/2).</summary>
/// <param name="Slots">Bag slots marked for sale.</param>
/// <param name="SaleKind">Kind of sale, the last byte of the packet.</param>
public sealed record SellRequest(
    IReadOnlyList<byte> Slots,
    byte SaleKind)
{
    /// <summary>Reads the request from a packet body; an empty body is a sale of nothing.</summary>
    /// <param name="body">The packet body after the action and sub code.</param>
    /// <returns>The parsed request.</returns>
    public static SellRequest Parse(ReadOnlySpan<byte> body) =>
        body.Length == 0 ? new SellRequest([], 0) : new SellRequest(body[..^1].ToArray(), body[^1]);
}
