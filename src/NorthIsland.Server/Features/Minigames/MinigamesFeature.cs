using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace NorthIsland.Server.Features.Minigames;

/// <summary>Registers the minigames feature.</summary>
public static class MinigamesFeature
{
    /// <summary>Registers the minigame options and the carnival, Q&amp;A, Golden Compass and Five in a Row handlers.</summary>
    /// <param name="services">The service collection.</param>
    /// <param name="section">The configuration section bound to <see cref="MinigameOptions"/>.</param>
    /// <returns>The service collection.</returns>
    public static IServiceCollection AddMinigames(this IServiceCollection services, IConfiguration section)
    {
        services.Configure<MinigameOptions>(section);
        services.AddSingleton<CarnivalHandlers>();
        services.AddSingleton<QuizHandlers>();
        services.AddSingleton<CompassHandlers>();
        services.AddSingleton<GobangHandlers>();
        return services;
    }
}
