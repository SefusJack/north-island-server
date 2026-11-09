using NorthIsland.Protocol.IO;
using NorthIsland.Protocol.Text;

namespace NorthIsland.Protocol.Messages.Login;

/// <summary>The automatic re-login with a numeric login id.</summary>
/// <param name="LoginNumber">The numeric login id.</param>
/// <param name="Prefix">The two-character prefix of the account name.</param>
/// <param name="Version">The client version.</param>
/// <param name="Password">The password.</param>
/// <param name="ItemDatSize">The decoded size of the client's item data file, null when unreadable.</param>
public sealed record NumericLoginRequest(
    int LoginNumber,
    string Prefix,
    ushort Version,
    string Password,
    long? ItemDatSize)
{
    /// <summary>Parses the request.</summary>
    /// <param name="body">The packet body.</param>
    /// <returns>The parsed request.</returns>
    public static NumericLoginRequest Parse(ReadOnlySpan<byte> body)
    {
        var r = new PacketReader(body);
        var passwordLength = r.U8();
        var loginNumber = r.I32();
        var prefix = GameEncoding.GetString(r.Bytes(2));
        var version = r.U16();
        var password = GameEncoding.GetString(r.Bytes(passwordLength));
        var itemDat = ItemDatSizeField.Read(ref r);
        return new NumericLoginRequest(loginNumber, prefix, version, password, itemDat);
    }
}
