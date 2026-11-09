namespace NorthIsland.Server.Net;

/// <summary>Counts the packets sent to a session while one incoming packet is handled.</summary>
/// <param name="session">The session being counted.</param>
public sealed class DispatchCount(
    GameSession session)
{
    /// <summary>The session being counted.</summary>
    public GameSession Session { get; } = session;

    /// <summary>The number of packets sent so far.</summary>
    public int Sent { get; set; }
}
