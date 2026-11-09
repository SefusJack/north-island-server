using System.Diagnostics.CodeAnalysis;

namespace NorthIsland.Server.Features.Inn;

/// <summary>Extension members that give a session its inn state.</summary>
public static class InnSessionStateExtensions
{
    /// <summary>Inn members of a session.</summary>
    /// <param name="session">The player's session.</param>
    extension(
        [NotNull] GameSession session)
    {
        /// <summary>The inn state of the session.</summary>
        public InnSessionState Inn => session.Feature<InnSessionState>();
    }
}
