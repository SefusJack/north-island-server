using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace NorthIsland.Server.Features.Vehicles;

/// <summary>Service registration of the vehicles feature.</summary>
public static class VehiclesFeature
{
    /// <summary>Binds the vehicle options and registers the vehicle handlers as singletons.</summary>
    /// <param name="services">The service collection.</param>
    /// <param name="section">The configuration section that holds the vehicle options.</param>
    /// <returns>The service collection.</returns>
    public static IServiceCollection AddVehicles(this IServiceCollection services, IConfiguration section)
    {
        services.Configure<VehicleFeatureOptions>(section);
        services.AddSingleton<VehicleHandlers>();
        services.AddSingleton<CapsuleHandlers>();
        services.AddSingleton<VehicleCareHandlers>();
        services.AddSingleton<ParkingHandlers>();
        services.AddSingleton<FuelBankHandlers>();
        services.AddSingleton<GarageHandlers>();
        services.AddSingleton<VehicleRepairHandlers>();
        return services;
    }
}
