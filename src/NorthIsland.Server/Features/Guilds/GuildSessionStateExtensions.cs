using System.Diagnostics.CodeAnalysis;

namespace NorthIsland.Server.Features.Guilds;

/// <summary>Access to the guild state of a session.</summary>
public static class GuildSessionStateExtensions
{
    /// <summary>Extensions on a session.</summary>
    /// <param name="session">The session.</param>
    extension(
        [NotNull] GameSession session)
    {
        /// <summary>The guild offers of this session.</summary>
        public GuildSessionState GuildOffers => session.Feature<GuildSessionState>();
    }
}
