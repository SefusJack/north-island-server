using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace NorthIsland.Server.Actions;

/// <summary>Registers the actions with the service collection.</summary>
public static class ActionsRegistration
{
    /// <summary>Adds the packet router as a singleton; creating it starts the features and registers every action.</summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection, so calls can be chained.</returns>
    public static IServiceCollection AddActions(this IServiceCollection services)
    {
        services.AddSingleton(sp =>
        {
            var router = new PacketRouter(sp.GetRequiredService<ILogger<PacketRouter>>());
            FeatureStartup.Start(sp);
            ActionDispatcher.Register(sp, router);
            return router;
        });
        return services;
    }
}
