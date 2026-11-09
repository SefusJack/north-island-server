using Microsoft.Extensions.DependencyInjection;

namespace NorthIsland.Server.Features.Crafting;

/// <summary>Registers the crafting handlers.</summary>
public static class CraftingFeature
{
    /// <summary>Adds the compound, fusion, pose, Chaos Crystal and dismantle handlers as singletons.</summary>
    /// <param name="services">The service collection to add to.</param>
    /// <returns>The same service collection.</returns>
    public static IServiceCollection AddCrafting(this IServiceCollection services)
    {
        services.AddSingleton<CompoundHandlers>();
        services.AddSingleton<FusionHandlers>();
        services.AddSingleton<PoseHandlers>();
        services.AddSingleton<ConfuseStoneHandlers>();
        services.AddSingleton<DismantleHandlers>();
        return services;
    }
}
