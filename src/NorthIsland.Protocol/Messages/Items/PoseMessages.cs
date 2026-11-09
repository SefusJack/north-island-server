using NorthIsland.Protocol.IO;
using NorthIsland.Protocol.Opcodes;

namespace NorthIsland.Protocol.Messages.Items;

/// <summary>Builders for the pose design packets (23 Pose).</summary>
public static class PoseMessages
{
    /// <summary>Builds the packet that reports a designed pose look.</summary>
    /// <param name="slot">Slot of the item.</param>
    /// <param name="look">Item id of the look.</param>
    /// <returns>The packet.</returns>
    public static byte[] Designed(byte slot, ushort look) => new PacketWriter(Action23.Code, Action23.Pose).U8(1).U8(slot).U16(look).ToArray();

    /// <summary>Builds the packet that reports a failed design.</summary>
    /// <returns>The packet.</returns>
    public static byte[] Failed() => new PacketWriter(Action23.Code, Action23.Pose).U8(2).ToArray();

    /// <summary>Builds the packet that reports the look was reverted.</summary>
    /// <returns>The packet.</returns>
    public static byte[] Reverted() => new PacketWriter(Action23.Code, Action23.Pose).U8(3).ToArray();

    /// <summary>Builds the packet that reports a failed revert.</summary>
    /// <returns>The packet.</returns>
    public static byte[] RevertFailed() => new PacketWriter(Action23.Code, Action23.Pose).U8(4).ToArray();
}
