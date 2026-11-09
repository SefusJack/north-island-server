using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace NorthIsland.Server.Features.Progression;

/// <summary>Registration of the progression feature (stat points and careers).</summary>
public static class ProgressionFeature
{
    /// <summary>Registers the progression options and handlers.</summary>
    /// <param name="services">The service collection.</param>
    /// <param name="section">The configuration section for <see cref="ProgressionOptions"/>.</param>
    /// <returns>The same service collection.</returns>
    public static IServiceCollection AddProgression(this IServiceCollection services, IConfiguration section)
    {
        services.Configure<ProgressionOptions>(section);
        services.AddSingleton<StatHandlers>();
        services.AddSingleton<CareerHandlers>();
        return services;
    }
}
