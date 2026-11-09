using NorthIsland.Protocol.IO;

namespace NorthIsland.Protocol.Messages.Login;

/// <summary>The login with an account name and password.</summary>
/// <param name="Account">The account name.</param>
/// <param name="Password">The password.</param>
/// <param name="Version">The client version.</param>
/// <param name="ItemDatSize">The decoded size of the client's item data file, null when unreadable.</param>
public sealed record AccountLoginRequest(
    string Account,
    string Password,
    ushort Version,
    long? ItemDatSize)
{
    /// <summary>Parses the request.</summary>
    /// <param name="body">The packet body.</param>
    /// <returns>The parsed request.</returns>
    public static AccountLoginRequest Parse(ReadOnlySpan<byte> body)
    {
        var r = new PacketReader(body);
        var account = r.PStr8();
        var password = r.PStr8();
        var version = r.U16();
        var itemDat = ItemDatSizeField.Read(ref r);
        return new AccountLoginRequest(account, password, version, itemDat);
    }
}
