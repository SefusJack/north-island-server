using NorthIsland.Protocol.IO;
using NorthIsland.Protocol.Opcodes;

namespace NorthIsland.Protocol.Messages.Items;

/// <summary>Builders for the shop packets of action 27 (sell, buy, open sell window).</summary>
public static class ShopMessages
{
    /// <summary>Result of a sale to a shop (<c>27/2</c>).</summary>
    /// <param name="result">The outcome sent to the client.</param>
    /// <returns>The packet.</returns>
    public static byte[] SellResult(SellResult result) => new PacketWriter(Action27.Code, Action27.Sell).U8((byte)result).ToArray();

    /// <summary>Opens the sell window.</summary>
    /// <param name="equipment">True for the equipment sell window, false for the goods one.</param>
    /// <returns>The packet.</returns>
    public static byte[] OpenSell(bool equipment) =>
        new PacketWriter(Action27.Code, equipment ? Action27.OpenSellEquipment : Action27.OpenSellGoods).ToArray();

    /// <summary>Result of a purchase from a shop (<c>27/1</c>).</summary>
    /// <param name="result">The outcome sent to the client; anything but 0 shows a message.</param>
    /// <returns>The packet.</returns>
    public static byte[] BuyResult(BuyResult result) => new PacketWriter(Action27.Code, Action27.Buy).U8((byte)result).ToArray();
}
