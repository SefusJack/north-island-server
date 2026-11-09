using NorthIsland.Protocol.IO;
using NorthIsland.Protocol.Opcodes;

namespace NorthIsland.Protocol.Messages.Items;

/// <summary>Builders for the item mall packets (actions 35, 75 and 80).</summary>
public static class MallMessages
{
    /// <summary>Builds the 35 packet that sets the player's mall points.</summary>
    /// <param name="points">The points.</param>
    /// <returns>The packet.</returns>
    public static byte[] Points(int points) => new PacketWriter(Action35.Code, Action35.Points).I32(points).I32(0).F64(0).ToArray();

    /// <summary>Builds the 75 catalog packet.</summary>
    /// <param name="rows">The items for sale.</param>
    /// <param name="client">Client generation; the 2014 client also gets the row keys.</param>
    /// <returns>The packet.</returns>
    public static byte[] Catalog(IReadOnlyCollection<MallRow> rows, ClientGeneration client)
    {
        var w = new PacketWriter(Action75.Code, Action75.Catalog).U16((ushort)rows.Count);
        foreach (var r in rows)
        {
            w.U16(r.ItemId).U8(r.Pack).U16(r.Price).U8(r.Discount).U8(r.Hot).U8(r.Category);
            if (client == ClientGeneration.Client2014)
                w.U16(r.Key);
        }
        return w.ToArray();
    }

    /// <summary>Builds the 75 packet that reports the result of a purchase.</summary>
    /// <param name="itemId">Id of the bought item.</param>
    /// <param name="category">Category of the row.</param>
    /// <param name="count">Number bought.</param>
    /// <param name="key">Row key, sent to the 2014 client only.</param>
    /// <param name="ok">True when the purchase went through.</param>
    /// <param name="client">Client generation.</param>
    /// <returns>The packet.</returns>
    public static byte[] Bought(
        ushort itemId,
        byte category,
        byte count,
        ushort key,
        bool ok,
        ClientGeneration client)
    {
        var w = new PacketWriter(Action75.Code, Action75.Bought).U16(itemId).U8(category).U8(count);
        if (client == ClientGeneration.Client2014)
            w.U16(key);
        return w.Bool(ok).ToArray();
    }

    /// <summary>Builds the second catalog packet, which always carries the row keys.</summary>
    /// <param name="rows">The items for sale.</param>
    /// <returns>The packet.</returns>
    public static byte[] CatalogB(IReadOnlyCollection<MallRow> rows)
    {
        var w = new PacketWriter(Action75.Code, Action75.CatalogB).U16((ushort)rows.Count);
        foreach (var r in rows)
            w.U16(r.ItemId).U8(r.Pack).U16(r.Price).U8(r.Discount).U8(r.Hot).U8(r.Category).U16(r.Key);
        return w.ToArray();
    }

    /// <summary>Builds the packet that sets the bonus points, capped at 65535.</summary>
    /// <param name="points">The bonus points.</param>
    /// <returns>The packet.</returns>
    public static byte[] Bonus(int points) => new PacketWriter(Action75.Code, Action75.Bonus).U16((ushort)Math.Clamp(points, 0, ushort.MaxValue)).ToArray();

    /// <summary>Builds the packet that reports a purchase paid with bonus points.</summary>
    /// <param name="left">Bonus points left.</param>
    /// <param name="spent">Bonus points spent.</param>
    /// <param name="itemId">Id of the bought item.</param>
    /// <param name="count">Number bought.</param>
    /// <returns>The packet.</returns>
    public static byte[] BonusDeducted(int left, int spent, ushort itemId, byte count) =>
        new PacketWriter(Action75.Code, Action75.BonusDeducted).I32(left).I32(spent).U16(itemId).U8(count).ToArray();

    /// <summary>Builds the packet that reports a purchase paid with mall points.</summary>
    /// <param name="left">Points left.</param>
    /// <param name="spent">Points spent.</param>
    /// <param name="itemId">Id of the bought item.</param>
    /// <param name="count">Number bought.</param>
    /// <returns>The packet.</returns>
    public static byte[] Deducted(int left, int spent, ushort itemId, byte count) =>
        new PacketWriter(Action75.Code, Action75.Deducted).I32(left).I32(spent).U16(itemId).U8(count).ToArray();

    /// <summary>Builds the packet that reports a refused purchase.</summary>
    /// <param name="code">Refusal code.</param>
    /// <returns>The packet.</returns>
    public static byte[] Refused(byte code) => new PacketWriter(Action75.Code, Action75.Refused).U8(code).ToArray();

    /// <summary>Builds 75/7, which opens or keeps shut the Game 777 tab.</summary>
    /// <param name="open">True to open the tab.</param>
    /// <returns>The packet.</returns>
    public static byte[] GameTab(bool open) => new PacketWriter(Action75.Code, Action75.GameTab).Bool(open).ToArray();

    /// <summary>Builds 75/6 with the failed enhancement notice.</summary>
    /// <returns>The packet.</returns>
    public static byte[] ForgeFailed() => new PacketWriter(Action75.Code, Action75.ForgeResult).U8(8).ToArray();

    /// <summary>Builds the 80 point store list.</summary>
    /// <param name="rows">The items for sale.</param>
    /// <returns>The packet.</returns>
    public static byte[] PointStore(IReadOnlyCollection<PointStoreRow> rows)
    {
        var w = new PacketWriter(Action80.Code, Action80.List).U8((byte)rows.Count);
        foreach (var r in rows)
            w.U16(r.ItemId).I32(r.Price).U8(r.Count);
        return w.ToArray();
    }

    /// <summary>Builds the 80 packet that reports a point store purchase.</summary>
    /// <param name="itemId">Id of the bought item.</param>
    /// <param name="spent">Points spent.</param>
    /// <param name="count">Number bought.</param>
    /// <returns>The packet.</returns>
    public static byte[] PointStoreBought(ushort itemId, int spent, byte count) =>
        new PacketWriter(Action80.Code, Action80.Bought).U16(itemId).I32(spent).U8(count).ToArray();

    /// <summary>Builds 80/8, which tells the client the server is busy.</summary>
    /// <returns>The packet.</returns>
    public static byte[] PointStoreBusy() => new PacketWriter(Action80.Code, Action80.Busy).ToArray();

    /// <summary>Builds the 80 packet that reports not enough points.</summary>
    /// <returns>The packet.</returns>
    public static byte[] PointStoreNotEnough() => new PacketWriter(Action80.Code, Action80.NotEnough).ToArray();
}
