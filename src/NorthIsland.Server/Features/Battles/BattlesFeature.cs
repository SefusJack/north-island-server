using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace NorthIsland.Server.Features.Battles;

/// <summary>Registration of the battle services.</summary>
public static class BattlesFeature
{
    /// <summary>Registers the battle options, the field encounters and the battle, training island and auto play handlers.</summary>
    /// <param name="services">The service collection to add to.</param>
    /// <param name="section">The configuration section that binds to <see cref="BattleOptions"/>.</param>
    /// <returns>The same service collection.</returns>
    public static IServiceCollection AddBattles(this IServiceCollection services, IConfiguration section)
    {
        services.Configure<BattleOptions>(section);
        services.AddSingleton<NorthIsland.Server.Features.Battles.FieldEncounters>();
        services.AddSingleton<BattleHandlers>();
        services.AddSingleton<TrainingIslandHandlers>();
        services.AddSingleton<AutoPlayHandlers>();
        return services;
    }
}
