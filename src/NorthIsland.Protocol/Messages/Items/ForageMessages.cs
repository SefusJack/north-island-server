using NorthIsland.Protocol.IO;
using NorthIsland.Protocol.Opcodes;

namespace NorthIsland.Protocol.Messages.Items;

/// <summary>Builders for the pet foraging packets (action 23).</summary>
public static class ForageMessages
{
    /// <summary>Builds the packet that reports an item a foraging pet found.</summary>
    /// <param name="item">Id of the found item.</param>
    /// <param name="count">Number of items found.</param>
    /// <returns>The packet.</returns>
    public static byte[] Found(ushort item, byte count) => new PacketWriter(Action23.Code, Action23.ForageFound).U16(item).U8(count).ToArray();

    /// <summary>Builds the packet that ends foraging.</summary>
    /// <param name="boneSlot">Bag slot of the bone used.</param>
    /// <param name="petSlot">Slot of the foraging pet.</param>
    /// <param name="failed">True when the foraging failed.</param>
    /// <returns>The packet.</returns>
    public static byte[] End(byte boneSlot, byte petSlot, bool failed) =>
        new PacketWriter(Action23.Code, Action23.ForageEnd).U8(boneSlot).U8(petSlot).U8(failed ? (byte)1 : (byte)2).ToArray();

    /// <summary>Builds the packet that reports the bag slot of the bone used for foraging.</summary>
    /// <param name="boneSlot">Bag slot of the bone.</param>
    /// <returns>The packet.</returns>
    public static byte[] Bone(byte boneSlot) => new PacketWriter(Action23.Code, Action23.ForageBone).U8(boneSlot).ToArray();
}
