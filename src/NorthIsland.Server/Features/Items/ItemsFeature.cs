using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace NorthIsland.Server.Features.Items;

/// <summary>Registers the items feature.</summary>
public static class ItemsFeature
{
    /// <summary>Registers the item options and the item handlers.</summary>
    /// <param name="services">The service collection.</param>
    /// <param name="section">The configuration section of the items feature.</param>
    /// <returns>The service collection.</returns>
    public static IServiceCollection AddItems(this IServiceCollection services, IConfiguration section)
    {
        services.Configure<ItemOptions>(section);
        services.AddSingleton<ItemHandlers>();
        services.AddSingleton<MapUseHandlers>();
        services.AddSingleton<DyeHandlers>();
        services.AddSingleton<GiftCardHandlers>();
        services.AddSingleton<BodyShiftHandlers>();
        services.AddSingleton<BottleHandlers>();
        services.AddSingleton<ReferHandlers>();
        services.AddSingleton<TimeIconHandlers>();
        services.AddSingleton<ExchangeVoucherHandlers>();
        services.AddSingleton<ShowItemHandlers>();
        services.AddSingleton<RepairHandlers>();
        services.AddSingleton<ItemEffectHandlers>();
        return services;
    }
}
