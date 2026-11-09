namespace NorthIsland.Server.Data;

/// <summary>A memory card a helper holds for a player who asked the helper to play on their behalf.</summary>
public sealed class PuppetCard
{
    /// <summary>Login id of the player who made the request.</summary>
    public int RequesterId { get; set; }

    /// <summary>Card slot at the helper, 1-based.</summary>
    public byte Slot { get; set; }

    /// <summary>When the card was accepted.</summary>
    public DateTime Since { get; set; }
}
