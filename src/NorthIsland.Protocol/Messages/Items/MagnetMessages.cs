using NorthIsland.Protocol.IO;
using NorthIsland.Protocol.Opcodes;

namespace NorthIsland.Protocol.Messages.Items;

/// <summary>Builders for the magnet notices (23 MagnetNotice).</summary>
public static class MagnetMessages
{
    /// <summary>Builds the notice that a player asks to use a magnet.</summary>
    /// <param name="requesterId">Id of the asking player.</param>
    /// <returns>The packet.</returns>
    public static byte[] Request(int requesterId) => new PacketWriter(Action23.Code, Action23.MagnetNotice).U8(1).I32(requesterId).ToArray();

    /// <summary>Builds the notice that the request waits for the other player.</summary>
    /// <param name="targetId">Id of the other player.</param>
    /// <param name="slot">Bag slot of the magnet.</param>
    /// <returns>The packet.</returns>
    public static byte[] Waiting(int targetId, byte slot) => new PacketWriter(Action23.Code, Action23.MagnetNotice).U8(2).I32(targetId).U8(slot).ToArray();

    /// <summary>Builds the notice with the result of a magnet request.</summary>
    /// <param name="code">Result code.</param>
    /// <returns>The packet.</returns>
    public static byte[] Result(byte code) => new PacketWriter(Action23.Code, Action23.MagnetNotice).U8(3).U8(code).ToArray();
}
