using System.Diagnostics.CodeAnalysis;

namespace NorthIsland.Server.Features.Houses;

/// <summary>Access to the house life state of a session.</summary>
public static class HouseLifeSessionStateExtensions
{
    /// <summary>Extensions on a session.</summary>
    /// <param name="session">The session.</param>
    extension(
        [NotNull] GameSession session)
    {
        /// <summary>The house life state of this session.</summary>
        public HouseLifeSessionState HouseLife => session.Feature<HouseLifeSessionState>();
    }
}
