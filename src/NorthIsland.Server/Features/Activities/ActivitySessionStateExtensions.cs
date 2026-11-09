using System.Diagnostics.CodeAnalysis;

namespace NorthIsland.Server.Features.Activities;

/// <summary>Extension members of <see cref="GameSession"/> for the activities feature.</summary>
public static class ActivitySessionStateExtensions
{
    /// <summary>Members for a session.</summary>
    /// <param name="session">The player's session.</param>
    extension(
        [NotNull] GameSession session)
    {
        /// <summary>The session's <see cref="ActivitySessionState"/>.</summary>
        public ActivitySessionState Activities => session.Feature<ActivitySessionState>();
    }
}
