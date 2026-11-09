using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace NorthIsland.Server.Features.Mall;

/// <summary>Registration of the mall services.</summary>
public static class MallFeature
{
    /// <summary>Adds the mall options and the mall, box and present case handlers.</summary>
    /// <param name="services">The service collection to add to.</param>
    /// <param name="section">The configuration section bound to <see cref="MallOptions"/>.</param>
    /// <returns>The same collection, for chaining.</returns>
    public static IServiceCollection AddMall(this IServiceCollection services, IConfiguration section)
    {
        services.Configure<MallOptions>(section);
        services.AddSingleton<MallHandlers>();
        services.AddSingleton<MallBoxHandlers>();
        services.AddSingleton<PresentCaseHandlers>();
        return services;
    }
}
