namespace NorthIsland.Server.Features.Items;

/// <summary>The show item state of one session.</summary>
public sealed class ShowItemSessionState
{
    /// <summary>The time until which the player's last firework runs.</summary>
    public DateTime FireworkUntil { get; set; }
}
