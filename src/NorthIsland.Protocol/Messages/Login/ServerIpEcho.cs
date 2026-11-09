using NorthIsland.Protocol.IO;

namespace NorthIsland.Protocol.Messages.Login;

/// <summary>The client's report of the server address it connected to.</summary>
/// <param name="Address">The address text.</param>
public sealed record ServerIpEcho(
    string Address)
{
    /// <summary>Parses the report; the whole body is the address.</summary>
    /// <param name="body">The packet body.</param>
    /// <returns>The parsed report.</returns>
    public static ServerIpEcho Parse(ReadOnlySpan<byte> body) => new(new PacketReader(body).RestString());
}
