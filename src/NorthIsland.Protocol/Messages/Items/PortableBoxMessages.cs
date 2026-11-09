using NorthIsland.Protocol.IO;
using NorthIsland.Protocol.Opcodes;

namespace NorthIsland.Protocol.Messages.Items;

/// <summary>Builders for the portable box packets: the experience bag and the picnic box (action 23).</summary>
public static class PortableBoxMessages
{
    /// <summary>Builds the packet that puts an item into a box cell.</summary>
    /// <param name="picnic">True for the picnic box, false for the experience bag.</param>
    /// <param name="cell">Cell number.</param>
    /// <param name="itemId">Id of the item.</param>
    /// <param name="count">Number of items.</param>
    /// <param name="notify">True to show a notice.</param>
    /// <returns>The packet.</returns>
    public static byte[] Added(
        bool picnic,
        byte cell,
        ushort itemId,
        byte count,
        bool notify = false) =>
            new PacketWriter(Action23.Code, picnic ? Action23.PicnicNotice : Action23.ExpBagNotice)
                .U8(1)
                .U8(cell)
                .U16(itemId)
                .U8(count)
                .U8(notify ? (byte)1 : (byte)0)
                .ToArray();

    /// <summary>Builds the packet that empties a box cell.</summary>
    /// <param name="picnic">True for the picnic box, false for the experience bag.</param>
    /// <param name="cell">Cell number.</param>
    /// <returns>The packet.</returns>
    public static byte[] Removed(bool picnic, byte cell) =>
        new PacketWriter(Action23.Code, picnic ? Action23.PicnicNotice : Action23.ExpBagNotice).U8(2).U8(cell).ToArray();

    /// <summary>Builds the packet that reports an experience bag cell was used.</summary>
    /// <param name="cell">Cell number.</param>
    /// <returns>The packet.</returns>
    public static byte[] ExpUsed(byte cell) => new PacketWriter(Action23.Code, Action23.ExpBagNotice).U8(3).U8(cell).ToArray();

    /// <summary>Builds the packet that reports a picnic box cell was used on a target.</summary>
    /// <param name="cell">Cell number.</param>
    /// <param name="target">Target byte.</param>
    /// <param name="hp">HP restored.</param>
    /// <param name="sp">SP restored.</param>
    /// <returns>The packet.</returns>
    public static byte[] PicnicUsed(byte cell, byte target, ushort hp, ushort sp) =>
        new PacketWriter(Action23.Code, Action23.PicnicNotice).U8(3).U8(cell).U8(target).U16(hp).U16(sp).ToArray();
}
