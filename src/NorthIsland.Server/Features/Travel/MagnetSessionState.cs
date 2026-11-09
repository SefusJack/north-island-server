namespace NorthIsland.Server.Features.Travel;

/// <summary>The magnet request a session takes part in.</summary>
public sealed class MagnetSessionState
{
    /// <summary>The login id of the player this player asked to be pulled to them, 0 for none.</summary>
    public int Target { get; set; }

    /// <summary>The login id of the player who asked this player to come, 0 for none.</summary>
    public int Requester { get; set; }
}
