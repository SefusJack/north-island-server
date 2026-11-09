namespace NorthIsland.Server.Features.PetCare;

/// <summary>The pet jobs kept on a player's session.</summary>
public sealed class PetJobSessionState
{
    /// <summary>The pet search in progress; null when none.</summary>
    public ForageJob? Forage { get; set; }

    /// <summary>The pet express trip in progress; null when none.</summary>
    public ExpressJob? Express { get; set; }
}
