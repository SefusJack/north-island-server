using System.Diagnostics.CodeAnalysis;

namespace NorthIsland.Server.Features.Battles;

/// <summary>Access to the encounter state of a session.</summary>
public static class EncounterSessionStateExtensions
{
    /// <summary>Extension members for a game session.</summary>
    /// <param name="session">The player's session.</param>
    extension(
        [NotNull] GameSession session)
    {
        /// <summary>The session's <see cref="EncounterSessionState"/>, created on first use.</summary>
        public EncounterSessionState Encounters => session.Feature<EncounterSessionState>();
    }
}
