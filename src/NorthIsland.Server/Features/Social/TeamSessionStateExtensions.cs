using System.Diagnostics.CodeAnalysis;

namespace NorthIsland.Server.Features.Social;

/// <summary>Session accessor for the team state.</summary>
public static class TeamSessionStateExtensions
{
    /// <summary>Extension members of a game session.</summary>
    /// <param name="session">The session.</param>
    extension(
        [NotNull] GameSession session)
    {
        /// <summary>The team state of the session.</summary>
        public TeamSessionState Team => session.Feature<TeamSessionState>();
    }
}
