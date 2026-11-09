using System.Diagnostics.CodeAnalysis;

namespace NorthIsland.Server.Features.Travel;

/// <summary>Adds the magnet state to a game session.</summary>
public static class MagnetSessionStateExtensions
{
    /// <summary>Extension members of a game session.</summary>
    /// <param name="session">The player's session.</param>
    extension(
        [NotNull] GameSession session)
    {
        /// <summary>The magnet state of the session.</summary>
        public MagnetSessionState Magnet => session.Feature<MagnetSessionState>();
    }
}
