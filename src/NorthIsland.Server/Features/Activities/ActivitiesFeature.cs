using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace NorthIsland.Server.Features.Activities;

/// <summary>Service registration of the activities feature (fishing, hot springs, activity list, online draw).</summary>
public static class ActivitiesFeature
{
    /// <summary>Binds <see cref="ActivityOptions"/> and registers the activity handlers as singletons.</summary>
    /// <param name="services">The service collection.</param>
    /// <param name="section">The configuration section holding the activity options.</param>
    /// <returns>The same service collection.</returns>
    public static IServiceCollection AddActivities(this IServiceCollection services, IConfiguration section)
    {
        services.Configure<ActivityOptions>(section);
        services.AddSingleton<FishingHandlers>();
        services.AddSingleton<HotSpringHandlers>();
        services.AddSingleton<ActivityHandlers>();
        services.AddSingleton<OnlineDrawHandlers>();
        return services;
    }
}
