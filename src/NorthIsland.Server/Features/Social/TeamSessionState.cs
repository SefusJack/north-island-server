namespace NorthIsland.Server.Features.Social;

/// <summary>Per-session state of the team feature.</summary>
public sealed class TeamSessionState
{
    /// <summary>Login id of the player who asked to join this player's team; 0 when none.</summary>
    public int PendingJoin { get; set; }

    /// <summary>Login id of the player who invited this player to a team; 0 when none.</summary>
    public int PendingInvite { get; set; }

    /// <summary>Login id of the leader who offered this player the lead; 0 when none.</summary>
    public int PendingLeaderOffer { get; set; }

    /// <summary>Whether the leader marked this player as counsellor.</summary>
    public bool Counsellor { get; set; }
}
