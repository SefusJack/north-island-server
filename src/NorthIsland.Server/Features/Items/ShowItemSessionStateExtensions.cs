using System.Diagnostics.CodeAnalysis;

namespace NorthIsland.Server.Features.Items;

/// <summary>Access to the show item state of a session.</summary>
public static class ShowItemSessionStateExtensions
{
    /// <summary>Extensions on a session.</summary>
    /// <param name="session">The player's session.</param>
    extension(
        [NotNull] GameSession session)
    {
        /// <summary>The session's show item state.</summary>
        public ShowItemSessionState ShowItems => session.Feature<ShowItemSessionState>();
    }
}
