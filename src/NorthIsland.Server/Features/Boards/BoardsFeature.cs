using Microsoft.Extensions.DependencyInjection;

namespace NorthIsland.Server.Features.Boards;

/// <summary>Registers the board and ranking services.</summary>
public static class BoardsFeature
{
    /// <summary>Adds the board and ranking registries and handlers as singletons.</summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The same service collection.</returns>
    public static IServiceCollection AddBoards(this IServiceCollection services)
    {
        services.AddSingleton<BoardRegistry>();
        services.AddSingleton<RankingRegistry>();
        services.AddSingleton<BoardHandlers>();
        services.AddSingleton<RankingHandlers>();
        return services;
    }
}
