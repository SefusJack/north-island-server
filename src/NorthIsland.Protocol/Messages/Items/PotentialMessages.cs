using NorthIsland.Protocol.IO;
using NorthIsland.Protocol.Opcodes;

namespace NorthIsland.Protocol.Messages.Items;

/// <summary>Builders for the pet potential packets (action 23).</summary>
public static class PotentialMessages
{
    /// <summary>Builds the packet that reports the potential of a pet.</summary>
    /// <param name="petSlot">Slot of the pet.</param>
    /// <param name="potential">The potential value.</param>
    /// <returns>The packet.</returns>
    public static byte[] Result(byte petSlot, byte potential) =>
        new PacketWriter(Action23.Code, Action23.PotentialResult).U8(1).U8(petSlot).U8(potential).ToArray();

    /// <summary>Builds the packet that reports a pet's potential was returned.</summary>
    /// <param name="petSlot">Slot of the pet.</param>
    /// <returns>The packet.</returns>
    public static byte[] ReturnResult(byte petSlot) => new PacketWriter(Action23.Code, Action23.PotentialReturned).U8(1).U8(petSlot).ToArray();
}
