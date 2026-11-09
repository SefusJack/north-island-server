namespace NorthIsland.Server.Features.Social;

/// <summary>Per-session state of the friend feature.</summary>
public sealed class FriendSessionState
{
    /// <summary>Login id of the player whose friend request is waiting for this player's answer; 0 when none.</summary>
    public int PendingFrom { get; set; }
}
