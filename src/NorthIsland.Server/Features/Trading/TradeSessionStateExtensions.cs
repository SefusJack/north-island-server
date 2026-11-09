using System.Diagnostics.CodeAnalysis;

namespace NorthIsland.Server.Features.Trading;

/// <summary>Adds the trade state to a game session.</summary>
public static class TradeSessionStateExtensions
{
    /// <summary>Extension members of a game session.</summary>
    /// <param name="session">The player's session.</param>
    extension(
        [NotNull] GameSession session)
    {
        /// <summary>The trade state of the session.</summary>
        public TradeSessionState Trades => session.Feature<TradeSessionState>();
    }
}
