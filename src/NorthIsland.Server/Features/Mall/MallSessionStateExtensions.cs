using System.Diagnostics.CodeAnalysis;

namespace NorthIsland.Server.Features.Mall;

/// <summary>Access to the mall state of a session.</summary>
public static class MallSessionStateExtensions
{
    /// <summary>Members on a game session.</summary>
    /// <param name="session">The session that holds the state.</param>
    extension(
        [NotNull] GameSession session)
    {
        /// <summary>The session's mall state, created on first use.</summary>
        public MallSessionState Mall => session.Feature<MallSessionState>();
    }
}
