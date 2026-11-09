using System.Diagnostics.CodeAnalysis;

namespace NorthIsland.Server.Features.Minigames;

/// <summary>Session accessors for the minigame state.</summary>
public static class MinigameSessionStateExtensions
{
    /// <summary>Minigame members of a game session.</summary>
    /// <param name="session">The player's session.</param>
    extension(
        [NotNull] GameSession session)
    {
        /// <summary>The session's minigame state, created on first use.</summary>
        public MinigameSessionState Minigames => session.Feature<MinigameSessionState>();

        /// <summary>The sport kind of the minigame the session is in: the event's pending game, else the item-mall game; null when none.</summary>
        public int? Minigame => session.Event?.PendingMinigame ?? session.Minigames.MallMinigame;
    }
}
