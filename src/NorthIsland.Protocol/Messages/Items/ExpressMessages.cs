using NorthIsland.Protocol.IO;
using NorthIsland.Protocol.Opcodes;

namespace NorthIsland.Protocol.Messages.Items;

/// <summary>Builders for the pet express packets (action 25).</summary>
public static class ExpressMessages
{
    /// <summary>Builds the packet that tells the client a pet express trip started or ended.</summary>
    /// <param name="petId">Id of the pet.</param>
    /// <param name="started">True when the trip started, false when it ended.</param>
    /// <returns>The packet.</returns>
    public static byte[] State(int petId, bool started) =>
        new PacketWriter(Action25.Code, Action25.Express).I32(petId).U8(started ? (byte)1 : (byte)2).ToArray();

    /// <summary>Builds the packet that tells the client how a pet walks after an express trip.</summary>
    /// <param name="petId">Id of the pet.</param>
    /// <param name="mode">Walking mode byte.</param>
    /// <returns>The packet.</returns>
    public static byte[] PetWalk(int petId, byte mode) => new PacketWriter(Action25.Code, Action25.ExpressStop).I32(petId).U8(mode).ToArray();

    /// <summary>Builds the packet that reports an express error.</summary>
    /// <param name="code">Error code; code 1 also carries an item id.</param>
    /// <param name="item">Id of the item the error is about, sent for code 1 only.</param>
    /// <returns>The packet.</returns>
    public static byte[] Error(byte code, ushort item = 0)
    {
        var w = new PacketWriter(Action25.Code, Action25.ExpressError).U8(code);
        return code == 1 ? w.U16(item).ToArray() : w.ToArray();
    }

    /// <summary>Builds the packet that reports the result of an express delivery.</summary>
    /// <param name="kind">Kind of result.</param>
    /// <param name="otherId">Id of the other player.</param>
    /// <param name="item">Id of the item.</param>
    /// <param name="count">Number of items.</param>
    /// <returns>The packet.</returns>
    public static byte[] Result(byte kind, int otherId, ushort item, byte count) =>
        new PacketWriter(Action25.Code, Action25.ExpressResult).U8(kind).I32(otherId).U16(item).U8(count).ToArray();
}
