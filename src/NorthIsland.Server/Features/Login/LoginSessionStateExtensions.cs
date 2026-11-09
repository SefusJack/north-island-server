using System.Diagnostics.CodeAnalysis;

namespace NorthIsland.Server.Features.Login;

/// <summary>Access to the login state of a session.</summary>
public static class LoginSessionStateExtensions
{
    /// <summary>Members on a game session.</summary>
    /// <param name="session">The session that holds the state.</param>
    extension(
        [NotNull] GameSession session)
    {
        /// <summary>The session's login state, created on first use.</summary>
        public LoginSessionState Login => session.Feature<LoginSessionState>();
    }
}
