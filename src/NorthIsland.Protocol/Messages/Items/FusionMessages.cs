using NorthIsland.Protocol.IO;
using NorthIsland.Protocol.Opcodes;

namespace NorthIsland.Protocol.Messages.Items;

/// <summary>Builders for the fusion (reinforce) result packets (23 ReinforceResult).</summary>
public static class FusionMessages
{
    /// <summary>Builds the packet that reports the new fusion level of a piece of gear.</summary>
    /// <param name="bagSlot">Bag slot of the gear.</param>
    /// <param name="level">The new fusion level.</param>
    /// <returns>The packet.</returns>
    public static byte[] Result(byte bagSlot, byte level) => new PacketWriter(Action23.Code, Action23.ReinforceResult).U8(1).U8(bagSlot).U8(level).ToArray();

    /// <summary>Builds the packet that reports a failed fusion.</summary>
    /// <returns>The packet.</returns>
    public static byte[] Failed() => new PacketWriter(Action23.Code, Action23.ReinforceResult).U8(2).ToArray();
}
