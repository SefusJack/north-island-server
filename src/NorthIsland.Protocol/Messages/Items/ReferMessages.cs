using NorthIsland.Protocol.IO;
using NorthIsland.Protocol.Opcodes;

namespace NorthIsland.Protocol.Messages.Items;

/// <summary>Builders for the refer result packets (23 ReferResult).</summary>
public static class ReferMessages
{
    /// <summary>Builds the packet with the result of a refer.</summary>
    /// <param name="first">First result byte.</param>
    /// <param name="second">Second result byte.</param>
    /// <param name="codes">Result code of each entry.</param>
    /// <returns>The packet.</returns>
    public static byte[] Result(byte first, byte second, byte[] codes) =>
        new PacketWriter(Action23.Code, Action23.ReferResult).U8(1).U8(first).U8(second).Bytes(codes).ToArray();

    /// <summary>Builds the packet that reports a refused refer.</summary>
    /// <returns>The packet.</returns>
    public static byte[] Refused() => new PacketWriter(Action23.Code, Action23.ReferResult).U8(2).ToArray();
}
