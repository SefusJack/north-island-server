using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace NorthIsland.Server.Features.Houses;

/// <summary>Registers the houses feature.</summary>
public static class HousesFeature
{
    /// <summary>Registers the house handlers: tents, manufacture, life, furniture, mail, beds, shops, rooms and the other house pieces.</summary>
    /// <param name="services">The service collection.</param>
    /// <param name="section">The configuration section of the feature.</param>
    /// <returns>The service collection.</returns>
    public static IServiceCollection AddHouses(this IServiceCollection services, IConfiguration section)
    {
        services.Configure<HouseOptions>(section);
        services.AddSingleton<TentHandlers>();
        services.AddSingleton<ManufactureHandlers>();
        services.AddSingleton<HouseLifeHandlers>();
        services.AddSingleton<HouseFurnitureHandlers>();
        services.AddSingleton<FurMailHandlers>();
        services.AddSingleton<BedHandlers>();
        services.AddSingleton<EggMachineHandlers>();
        services.AddSingleton<HomeShopHandlers>();
        services.AddSingleton<StoreroomHandlers>();
        services.AddSingleton<HouseDecorHandlers>();
        services.AddSingleton<HouseRoomHandlers>();
        services.AddSingleton<PillarHandlers>();
        services.AddSingleton<HouseFunHandlers>();
        services.AddSingleton<FishTankHandlers>();
        services.AddSingleton<GasHandlers>();
        services.AddSingleton<BlackboardHandlers>();
        services.AddSingleton<BookHandlers>();
        services.AddSingleton<MaidHandlers>();
        return services;
    }
}
