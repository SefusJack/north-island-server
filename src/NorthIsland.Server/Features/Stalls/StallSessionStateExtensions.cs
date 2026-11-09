using System.Diagnostics.CodeAnalysis;

namespace NorthIsland.Server.Features.Stalls;

/// <summary>Session accessor for the stall state.</summary>
public static class StallSessionStateExtensions
{
    /// <summary>Extension members of a game session.</summary>
    /// <param name="session">The session.</param>
    extension(
        [NotNull] GameSession session)
    {
        /// <summary>The stall state of the session.</summary>
        public StallSessionState Stalls => session.Feature<StallSessionState>();
    }
}
