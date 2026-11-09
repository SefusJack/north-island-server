using Microsoft.Extensions.DependencyInjection;

namespace NorthIsland.Server.Features.Inn;

/// <summary>Registers the inn handlers.</summary>
public static class InnFeature
{
    /// <summary>Adds the inn handlers to the service collection.</summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The same service collection.</returns>
    public static IServiceCollection AddInn(this IServiceCollection services)
    {
        services.AddSingleton<InnHandlers>();
        return services;
    }
}
