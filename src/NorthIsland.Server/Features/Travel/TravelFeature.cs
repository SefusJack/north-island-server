using Microsoft.Extensions.DependencyInjection;

namespace NorthIsland.Server.Features.Travel;

/// <summary>Registers the travel feature: teleport items, transmit vehicles, GPS and the Magic Magnet.</summary>
public static class TravelFeature
{
    /// <summary>Adds the travel handlers as singletons.</summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The same service collection.</returns>
    public static IServiceCollection AddTravel(this IServiceCollection services)
    {
        services.AddSingleton<TeleportHandlers>();
        services.AddSingleton<TransmitRoutes>();
        services.AddSingleton<TransmitHandlers>();
        services.AddSingleton<GpsHandlers>();
        services.AddSingleton<MagnetHandlers>();
        return services;
    }
}
