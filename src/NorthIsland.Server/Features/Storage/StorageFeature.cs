using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace NorthIsland.Server.Features.Storage;

/// <summary>Registers the storage feature: bank, ATM, warehouse, Item Bag, Space Contract and portable boxes.</summary>
public static class StorageFeature
{
    /// <summary>Binds the storage options and adds the storage handlers as singletons.</summary>
    /// <param name="services">The service collection.</param>
    /// <param name="section">The configuration section of the storage options.</param>
    /// <returns>The same service collection.</returns>
    public static IServiceCollection AddStorage(this IServiceCollection services, IConfiguration section)
    {
        services.Configure<StorageOptions>(section);
        services.AddSingleton<NorthIsland.Server.Features.Storage.SpaceContract>();
        services.AddSingleton<BankHandlers>();
        services.AddSingleton<AtmHandlers>();
        services.AddSingleton<WarehouseHandlers>();
        services.AddSingleton<MiniGridHandlers>();
        services.AddSingleton<SpaceContractHandlers>();
        services.AddSingleton<PortableBoxHandlers>();
        return services;
    }
}
