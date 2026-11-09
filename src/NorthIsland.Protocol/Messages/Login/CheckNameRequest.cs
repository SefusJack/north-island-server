using NorthIsland.Protocol.IO;

namespace NorthIsland.Protocol.Messages.Login;

/// <summary>A request to check whether a new character name can be used.</summary>
/// <param name="Name">The proposed name.</param>
public sealed record CheckNameRequest(
    string Name)
{
    /// <summary>Parses the request; the whole body is the name.</summary>
    /// <param name="body">The packet body.</param>
    /// <returns>The parsed request.</returns>
    public static CheckNameRequest Parse(ReadOnlySpan<byte> body) => new(new PacketReader(body).RestString());
}
