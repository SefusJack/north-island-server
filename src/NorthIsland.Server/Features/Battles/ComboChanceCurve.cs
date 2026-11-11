namespace NorthIsland.Server.Features.Battles;

/// <summary>How the chance of a combination attack grows with the level gap.</summary>
public enum ComboChanceCurve
{
    /// <summary>A fixed percentage per level-gap band (base, mid, high, full).</summary>
    Steps,

    /// <summary>A percentage that slides from the base value to 100 with the ratio of the levels.</summary>
    Sliding,
}
