using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace NorthIsland.Server.Features.Marriage;

/// <summary>Registration of the marriage services.</summary>
public static class MarriageFeature
{
    /// <summary>Adds the marriage options, the registry and the handlers.</summary>
    /// <param name="services">The service collection to add to.</param>
    /// <param name="section">The configuration section bound to <see cref="MarriageOptions"/>.</param>
    /// <returns>The same collection, for chaining.</returns>
    public static IServiceCollection AddMarriage(this IServiceCollection services, IConfiguration section)
    {
        services.Configure<MarriageOptions>(section);
        services.AddSingleton<MarriageRegistry>();
        services.AddSingleton<MarriageHandlers>();
        return services;
    }
}
