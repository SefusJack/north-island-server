namespace NorthIsland.Server.Net;

/// <summary>A handler for one kind of packet.</summary>
/// <param name="session">The session that sent the packet.</param>
/// <param name="body">The packet body after the action and sub code.</param>
public delegate ValueTask PacketHandler(GameSession session, ReadOnlyMemory<byte> body);
