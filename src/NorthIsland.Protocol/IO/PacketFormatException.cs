namespace NorthIsland.Protocol.IO;

/// <summary>Thrown by <see cref="PacketReader"/> when a packet is shorter than the reader expects.</summary>
/// <param name="message">What was missing, including the offset.</param>
public sealed class PacketFormatException(
    string message) : Exception(message);
