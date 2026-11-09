using NorthIsland.Protocol.IO;
using NorthIsland.Protocol.Opcodes;

namespace NorthIsland.Protocol.Messages.Items;

/// <summary>Builders for the bottle packets (23 BottleFill).</summary>
public static class BottleMessages
{
    /// <summary>Builds the packet that reports the amount a bottle in the bag now holds.</summary>
    /// <param name="bagSlot">Bag slot of the bottle.</param>
    /// <param name="amount">The amount.</param>
    /// <returns>The packet.</returns>
    public static byte[] Used(byte bagSlot, int amount) => new PacketWriter(Action23.Code, Action23.BottleFill).U8(1).U8(bagSlot).I32(amount).ToArray();

    /// <summary>Builds the packet that reports the amount stored in a pool.</summary>
    /// <param name="kind">Kind of pool.</param>
    /// <param name="amount">The amount.</param>
    /// <returns>The packet.</returns>
    public static byte[] Pool(byte kind, int amount) => new PacketWriter(Action23.Code, Action23.BottleFill).U8(2).U8(kind).I32(amount).ToArray();
}
