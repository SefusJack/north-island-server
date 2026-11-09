using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace NorthIsland.Server.Features.Guilds;

/// <summary>Registers the guilds feature.</summary>
public static class GuildsFeature
{
    /// <summary>Registers the guild registry, castle registry, guild handlers and the Trojan War schedule.</summary>
    /// <param name="services">The service collection.</param>
    /// <param name="section">The configuration section of the feature.</param>
    /// <returns>The service collection.</returns>
    public static IServiceCollection AddGuilds(this IServiceCollection services, IConfiguration section)
    {
        services.Configure<GuildFeatureOptions>(section);
        services.AddSingleton<GuildRegistry>();
        services.AddSingleton<CastleRegistry>();
        services.AddSingleton<GuildHandlers>();
        services.AddSingleton<GuildEconomyHandlers>();
        services.AddSingleton<NorthIsland.Server.Features.Guilds.TrojanWar>();
        return services;
    }
}
