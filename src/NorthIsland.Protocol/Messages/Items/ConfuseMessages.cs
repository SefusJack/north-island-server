using NorthIsland.Protocol.IO;
using NorthIsland.Protocol.Opcodes;

namespace NorthIsland.Protocol.Messages.Items;

/// <summary>Builder for the confuse result packet (23 ConfuseResult).</summary>
public static class ConfuseMessages
{
    /// <summary>Builds the packet that reports a confuse step for a piece of gear.</summary>
    /// <param name="step">The step reached.</param>
    /// <param name="bagSlot">Bag slot of the gear.</param>
    /// <param name="stone">Id of the stone set in the gear.</param>
    /// <returns>The packet.</returns>
    public static byte[] Result(byte step, byte bagSlot, ushort stone) =>
        new PacketWriter(Action23.Code, Action23.ConfuseResult).U8(1).U8(step).U8(bagSlot).U16(stone).ToArray();
}
