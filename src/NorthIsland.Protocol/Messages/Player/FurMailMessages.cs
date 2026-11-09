using NorthIsland.Protocol.IO;
using NorthIsland.Protocol.Opcodes;

namespace NorthIsland.Protocol.Messages.Player;

/// <summary>Builders for the furniture mailbox packet (<see cref="Action14.FurMailPacket"/>).</summary>
public static class FurMailMessages
{
    /// <summary>Sends a mailbox result.</summary>
    /// <param name="result">The result code.</param>
    /// <returns>The packet.</returns>
    public static byte[] Result(byte result) => new PacketWriter(Action14.Code, Action14.FurMailPacket).U8(1).U8(result).ToArray();

    /// <summary>Delivers a letter from the mailbox.</summary>
    /// <param name="senderId">Id of the sender.</param>
    /// <param name="senderName">Name of the sender.</param>
    /// <param name="time">Send time as an OLE automation date.</param>
    /// <param name="text">The letter text.</param>
    /// <returns>The packet.</returns>
    public static byte[] Letter(int senderId, string senderName, double time, string text) =>
        new PacketWriter(Action14.Code, Action14.FurMailPacket).U8(2).I32(senderId).PStr8(senderName).F64(time).RawString(text).ToArray();
}
