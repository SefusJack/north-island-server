using NorthIsland.Protocol.IO;

namespace NorthIsland.Protocol.Messages.Login;

/// <summary>A request to enter the game with one character.</summary>
/// <param name="Slot">The character's slot.</param>
public sealed record SelectSlotRequest(
    byte Slot)
{
    /// <summary>Parses the request.</summary>
    /// <param name="body">The packet body.</param>
    /// <returns>The parsed request.</returns>
    public static SelectSlotRequest Parse(ReadOnlySpan<byte> body) => new(new PacketReader(body).U8());
}
