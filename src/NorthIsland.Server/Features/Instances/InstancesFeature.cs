using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace NorthIsland.Server.Features.Instances;

/// <summary>Registers the instance services.</summary>
public static class InstancesFeature
{
    /// <summary>Adds the instance options, registry and handlers to the service collection.</summary>
    /// <param name="services">The service collection.</param>
    /// <param name="section">The configuration section bound to the instance options.</param>
    /// <returns>The same service collection.</returns>
    public static IServiceCollection AddInstances(this IServiceCollection services, IConfiguration section)
    {
        services.Configure<InstanceOptions>(section);
        services.AddSingleton<NorthIsland.Server.Features.Instances.InstanceRegistry>();
        services.AddSingleton<InstanceHandlers>();
        return services;
    }
}
