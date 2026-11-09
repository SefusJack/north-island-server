using NorthIsland.Protocol.IO;
using NorthIsland.Protocol.Opcodes;

namespace NorthIsland.Protocol.Messages.Items;

/// <summary>Builder for the dismantle result packet (23 DismantleResult).</summary>
public static class DismantleMessages
{
    /// <summary>Builds the packet that reports a dismantled piece of gear.</summary>
    /// <param name="bagSlot">Bag slot of the gear.</param>
    /// <param name="wear">Wear value of the gear.</param>
    /// <returns>The packet.</returns>
    public static byte[] Result(byte bagSlot, byte wear) =>
        new PacketWriter(Action23.Code, Action23.DismantleResult).U8(1).U8(bagSlot).I32(wear).U8(0).ToArray();
}
