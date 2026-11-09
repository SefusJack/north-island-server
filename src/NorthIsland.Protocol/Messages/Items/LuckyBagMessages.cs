using NorthIsland.Protocol.IO;
using NorthIsland.Protocol.Opcodes;

namespace NorthIsland.Protocol.Messages.Items;

/// <summary>Builder for the lucky bag info packet (action 91).</summary>
public static class LuckyBagMessages
{
    /// <summary>Builds a 91 packet with the contents of a lucky bag.</summary>
    /// <param name="bagId">Id of the lucky bag item.</param>
    /// <param name="rows">Item id and count of each content row; null sends the bag id alone.</param>
    /// <returns>The packet.</returns>
    public static byte[] Info(ushort bagId, IReadOnlyList<(ushort Item, byte Count)>? rows)
    {
        var w = new PacketWriter(Action91.Code, Action91.Info).U16(bagId);
        if (rows is null)
            return w.ToArray();
        w.U8((byte)rows.Count);
        foreach (var (item, count) in rows)
            w.U16(item).U8(count);
        return w.ToArray();
    }
}
