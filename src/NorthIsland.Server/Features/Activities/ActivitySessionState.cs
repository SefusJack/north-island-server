namespace NorthIsland.Server.Features.Activities;

/// <summary>Per-session state of the activities: the running fishing and hot spring loops.</summary>
public sealed class ActivitySessionState
{
    /// <summary>Cancellation source of the running fishing loop; null when the player is not fishing.</summary>
    public CancellationTokenSource? Fishing { get; set; }

    /// <summary>Cancellation source of the running hot spring loop; null when the player is not bathing.</summary>
    public CancellationTokenSource? Spring { get; set; }
}
