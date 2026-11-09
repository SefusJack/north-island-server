using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace NorthIsland.Server.Features.Trading;

/// <summary>Registers the trading feature: item trade and pet trade.</summary>
public static class TradingFeature
{
    /// <summary>Binds the trade options and adds the trade handlers as singletons.</summary>
    /// <param name="services">The service collection.</param>
    /// <param name="section">The configuration section of the trade options.</param>
    /// <returns>The same service collection.</returns>
    public static IServiceCollection AddTrading(this IServiceCollection services, IConfiguration section)
    {
        services.Configure<TradeOptions>(section);
        services.AddSingleton<TradeHandlers>();
        services.AddSingleton<PetTradeHandlers>();
        return services;
    }
}
