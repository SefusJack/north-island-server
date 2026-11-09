using NorthIsland.Protocol.IO;
using NorthIsland.Protocol.Opcodes;

namespace NorthIsland.Protocol.Messages.Items;

/// <summary>Builders for the exchange card packets (23 GiftCardReply).</summary>
public static class GiftCardMessages
{
    /// <summary>Builds the sub 2 packet with the full list of cards and their choices.</summary>
    /// <param name="cards">The cards.</param>
    /// <returns>The packet.</returns>
    public static byte[] Cards(IReadOnlyCollection<GiftCardEntry> cards)
    {
        var w = new PacketWriter(Action23.Code, Action23.GiftCardReply).U8(2).U16((ushort)cards.Count);
        foreach (var card in cards)
        {
            w.U16(card.Id).U8(card.Version).U8((byte)card.Choices.Count);
            foreach (var (item, amount) in card.Choices)
                w.U16(item).U8(amount);
        }
        return w.ToArray();
    }

    /// <summary>Builds the sub 1 packet with the version of each card.</summary>
    /// <param name="cards">Card id and version of each card.</param>
    /// <returns>The packet.</returns>
    public static byte[] Versions(IReadOnlyCollection<(ushort Card, byte Version)> cards)
    {
        var w = new PacketWriter(Action23.Code, Action23.GiftCardReply).U8(1).U16((ushort)cards.Count);
        foreach (var (card, version) in cards)
            w.U16(card).U8(version);
        return w.ToArray();
    }

    /// <summary>Builds the sub 4 packet that opens a card.</summary>
    /// <param name="slot">Bag slot of the card.</param>
    /// <returns>The packet.</returns>
    public static byte[] Open(byte slot) => new PacketWriter(Action23.Code, Action23.GiftCardReply).U8(4).U8(slot).ToArray();

    /// <summary>Builds the sub 3 packet that reports a card use.</summary>
    /// <param name="card">Id of the card.</param>
    /// <param name="times">How many times the card has been used.</param>
    /// <param name="item">Id of the item received.</param>
    /// <param name="amount">Number of items received.</param>
    /// <returns>The packet.</returns>
    public static byte[] Used(ushort card, byte times, ushort item, byte amount) =>
        new PacketWriter(Action23.Code, Action23.GiftCardReply).U8(3).U16(card).U8(times).U16(item).U8(amount).ToArray();
}
