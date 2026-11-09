using System.Diagnostics.CodeAnalysis;

namespace NorthIsland.Server.Features.PetCare;

/// <summary>Session accessors for the pet jobs.</summary>
public static class PetJobSessionStateExtensions
{
    /// <summary>Pet job members of a game session.</summary>
    /// <param name="session">The player's session.</param>
    extension(
        [NotNull] GameSession session)
    {
        /// <summary>The session's pet jobs, created on first use.</summary>
        public PetJobSessionState PetJobs => session.Feature<PetJobSessionState>();
    }
}
