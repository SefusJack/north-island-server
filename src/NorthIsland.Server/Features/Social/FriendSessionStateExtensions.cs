using System.Diagnostics.CodeAnalysis;

namespace NorthIsland.Server.Features.Social;

/// <summary>Session accessor for the friend state.</summary>
public static class FriendSessionStateExtensions
{
    /// <summary>Extension members of a game session.</summary>
    /// <param name="session">The session.</param>
    extension(
        [NotNull] GameSession session)
    {
        /// <summary>The friend state of the session.</summary>
        public FriendSessionState Friends => session.Feature<FriendSessionState>();
    }
}
