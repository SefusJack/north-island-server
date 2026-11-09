using Microsoft.Extensions.DependencyInjection;

namespace NorthIsland.Server.Features.Shops;

/// <summary>Registration of the shops feature.</summary>
public static class ShopsFeature
{
    /// <summary>Registers the shop handlers.</summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The same service collection.</returns>
    public static IServiceCollection AddShops(this IServiceCollection services)
    {
        services.AddSingleton<ShopHandlers>();
        return services;
    }
}
