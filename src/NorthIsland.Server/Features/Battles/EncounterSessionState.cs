namespace NorthIsland.Server.Features.Battles;

/// <summary>Per-session timers for random fights.</summary>
public sealed class EncounterSessionState
{
    /// <summary>No random field fight before this time (UTC).</summary>
    public DateTime FieldQuietUntil { get; set; }

    /// <summary>No new fight can start for the player before this time (UTC); set when a battle ends.</summary>
    public DateTime NoBattleBefore { get; set; }

    /// <summary>Tells whether the player is still protected from fights.</summary>
    /// <param name="now">The current UTC time.</param>
    /// <returns>True while <paramref name="now"/> is before <see cref="NoBattleBefore"/>.</returns>
    public bool Immune(DateTime now) => now < NoBattleBefore;
}
