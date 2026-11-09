using NorthIsland.Protocol.IO;

namespace NorthIsland.Protocol.Messages.Login;

/// <summary>A request to delete a character.</summary>
/// <param name="Slot">The character's slot.</param>
/// <param name="Password">The account password given for confirmation.</param>
/// <param name="Cipher">The cipher string given for confirmation.</param>
public sealed record DeleteCharacterRequest(
    byte Slot,
    string Password,
    string Cipher)
{
    /// <summary>Parses the request.</summary>
    /// <param name="body">The packet body.</param>
    /// <returns>The parsed request.</returns>
    public static DeleteCharacterRequest Parse(ReadOnlySpan<byte> body)
    {
        var r = new PacketReader(body);
        return new DeleteCharacterRequest(r.U8(), r.PStr8(), r.PStr8());
    }
}
