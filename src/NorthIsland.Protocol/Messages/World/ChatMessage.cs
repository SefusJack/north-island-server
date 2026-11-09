using NorthIsland.Protocol.IO;
using NorthIsland.Protocol.Opcodes;

namespace NorthIsland.Protocol.Messages.World;

/// <summary>A chat line the client sent on action 2; the sub code is the channel.</summary>
/// <param name="Channel">The channel (the sub code of the packet).</param>
/// <param name="TargetId">Id of the receiving player for a whisper, otherwise null.</param>
/// <param name="Text">The chat text.</param>
public sealed record ChatMessage(
    byte Channel,
    int? TargetId,
    string Text)
{
    /// <summary>Reads a chat line from a packet body.</summary>
    /// <param name="channel">The sub code of the packet.</param>
    /// <param name="body">The packet body after the action and sub code.</param>
    /// <returns>The parsed chat line.</returns>
    public static ChatMessage Parse(byte channel, ReadOnlySpan<byte> body)
    {
        var r = new PacketReader(body);
        int? target = channel == Action2.Whisper ? r.I32() : null;
        return new ChatMessage(channel, target, r.RestString());
    }
}
