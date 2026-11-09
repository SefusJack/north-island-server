using NorthIsland.Protocol.IO;
using NorthIsland.Protocol.Opcodes;

namespace NorthIsland.Protocol.Messages.Items;

/// <summary>Builders for the player-to-player trade packets of action 25, all server to client.</summary>
public static class TradeMessages
{
    /// <summary>Opens the trade window with a partner.</summary>
    /// <param name="partnerId">Login id of the trade partner.</param>
    /// <returns>The packet.</returns>
    public static byte[] Start(int partnerId) => new PacketWriter(Action25.Code, Action25.Start).I32(partnerId).ToArray();

    /// <summary>Opens the large trade window with a partner.</summary>
    /// <param name="partnerId">Login id of the trade partner.</param>
    /// <returns>The packet.</returns>
    public static byte[] LargeStart(int partnerId) => new PacketWriter(Action25.Code, Action25.LargeRequest).I32(partnerId).ToArray();

    /// <summary>Answers a large trade request or ends the large trade.</summary>
    /// <param name="code">The result code; most are the <see cref="TradeResult"/> values.</param>
    /// <returns>The packet.</returns>
    public static byte[] LargeResult(byte code) => new PacketWriter(Action25.Code, Action25.LargeDecide).U8(code).ToArray();

    /// <summary>Sends the partner's offer in a large trade: gold, then items with their amounts.</summary>
    /// <param name="gold">Gold offered.</param>
    /// <param name="items">Item id, used amount, extra amount and wear of each item.</param>
    /// <returns>The packet.</returns>
    public static byte[] LargeOffer(int gold, IEnumerable<(ushort ItemId, int Used, ushort Extra, byte Wear)> items)
    {
        var w = new PacketWriter(Action25.Code, Action25.LargeOffer).I32(gold);
        foreach (var i in items)
            w.U16(i.ItemId).I32(i.Used).U16(i.Extra).U8(i.Wear);
        return w.ToArray();
    }

    /// <summary>Tells the client whether the account has been online long enough to trade.</summary>
    /// <param name="enough">True when the time is enough.</param>
    /// <returns>The packet.</returns>
    public static byte[] OnlineTime(bool enough) =>
        new PacketWriter(Action25.Code, Action25.OnlineTime).U8(enough ? (byte)2 : (byte)1).ToArray();

    /// <summary>Ends or refuses a trade with a result code.</summary>
    /// <param name="result">The result.</param>
    /// <returns>The packet.</returns>
    public static byte[] Result(TradeResult result) => new PacketWriter(Action25.Code, Action25.Result).U8((byte)result).ToArray();

    /// <summary>Sends the partner's offer: gold, then the items.</summary>
    /// <param name="gold">Gold offered.</param>
    /// <param name="items">The items offered.</param>
    /// <returns>The packet.</returns>
    public static byte[] PartnerOffer(int gold, IEnumerable<TradeItem> items)
    {
        var w = new PacketWriter(Action25.Code, Action25.PartnerOffer).I32(gold);
        foreach (var i in items)
            w.U16(i.ItemId).U8(i.Count).U8(i.Wear).I32(0).U16(0).U8(0).U8(0).U16(0).F64(0).U8(0).U8(0).U16(0).U16(0);
        return w.ToArray();
    }
}
