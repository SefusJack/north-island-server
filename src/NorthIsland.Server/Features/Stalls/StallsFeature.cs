using Microsoft.Extensions.DependencyInjection;

namespace NorthIsland.Server.Features.Stalls;

/// <summary>Registration of the stalls feature.</summary>
public static class StallsFeature
{
    /// <summary>Registers the stall handlers.</summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The same service collection.</returns>
    public static IServiceCollection AddStalls(this IServiceCollection services)
    {
        services.AddSingleton<StallHandlers>();
        services.AddSingleton<PetStallHandlers>();
        services.AddSingleton<StallCaseHandlers>();
        services.AddSingleton<UtensilStallHandlers>();
        return services;
    }
}
