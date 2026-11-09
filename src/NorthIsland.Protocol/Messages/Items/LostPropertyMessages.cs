using NorthIsland.Protocol.IO;
using NorthIsland.Protocol.Opcodes;

namespace NorthIsland.Protocol.Messages.Items;

/// <summary>Builders for the Lost Property Office packets (action 86).</summary>
public static class LostPropertyMessages
{
    /// <summary>Builds an 86 packet with the list of items that can be reclaimed.</summary>
    /// <param name="records">The lost items.</param>
    /// <returns>The packet.</returns>
    public static byte[] List(IReadOnlyCollection<LostPropertyRecord> records)
    {
        var w = new PacketWriter(Action86.Code, Action86.List).U16((ushort)records.Count);
        foreach (var r in records)
            w.U16(r.ItemId).U8(0).U16(r.Insert).U8(r.Fusion).U8(0).U16(r.Pose).I32(r.Key);
        return w.ToArray();
    }

    /// <summary>Builds an 86 packet which reports an item was reclaimed.</summary>
    /// <param name="itemId">Id of the item.</param>
    /// <returns>The packet.</returns>
    public static byte[] Reclaimed(ushort itemId) => new PacketWriter(Action86.Code, Action86.Reclaim).U16(itemId).ToArray();

    /// <summary>Builds an 86 packet which reports a refusal.</summary>
    /// <param name="code">Refusal code.</param>
    /// <returns>The packet.</returns>
    public static byte[] Refused(byte code) => new PacketWriter(Action86.Code, Action86.Refused).U8(code).ToArray();
}
