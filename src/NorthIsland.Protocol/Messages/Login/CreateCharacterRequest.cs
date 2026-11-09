using NorthIsland.Protocol.IO;

namespace NorthIsland.Protocol.Messages.Login;

/// <summary>A request to create a character.</summary>
/// <param name="BodyType">Body type chosen.</param>
/// <param name="Reserved">A byte read from the packet that the server does not use.</param>
/// <param name="Head">Head chosen.</param>
/// <param name="Sprite">Sprite chosen.</param>
/// <param name="Colour1">First colour value.</param>
/// <param name="Colour2">Second colour value.</param>
/// <param name="Element">Element chosen.</param>
/// <param name="Stats">The five stat bytes chosen.</param>
/// <param name="Cipher">The cipher string, null when the packet ends before it.</param>
public sealed record CreateCharacterRequest(
    byte BodyType,
    byte Reserved,
    byte Head,
    byte Sprite,
    int Colour1,
    int Colour2,
    byte Element,
    byte[] Stats,
    string? Cipher)
{
    /// <summary>Parses the request.</summary>
    /// <param name="body">The packet body.</param>
    /// <returns>The parsed request.</returns>
    public static CreateCharacterRequest Parse(ReadOnlySpan<byte> body)
    {
        var r = new PacketReader(body);
        var bodyType = r.U8();
        var reserved = r.U8();
        var head = r.U8();
        var sprite = r.U8();
        var colour1 = r.I32();
        var colour2 = r.I32();
        var element = r.U8();
        var stats = r.Bytes(5).ToArray();
        var cipher = r.IsEmpty ? null : r.PStr8();
        return new CreateCharacterRequest(bodyType, reserved, head, sprite, colour1, colour2, element, stats, cipher);
    }
}
