namespace NorthIsland.Server.Features.Crafting;

/// <summary>The result of a compound.</summary>
public enum AlchemyOutcome
{
    /// <summary>An item of the rolled rank was made.</summary>
    Made,

    /// <summary>The compound failed and the materials are returned.</summary>
    NothingLost,

    /// <summary>The compound failed and the main material is replaced by a lower-rank item.</summary>
    Degraded,

    /// <summary>The compound failed and the materials are replaced by the common stone.</summary>
    Stone
}
