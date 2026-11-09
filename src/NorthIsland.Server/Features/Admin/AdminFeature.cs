using Microsoft.Extensions.DependencyInjection;

namespace NorthIsland.Server.Features.Admin;

/// <summary>Service registration of the admin feature (GM commands and the ban list).</summary>
public static class AdminFeature
{
    /// <summary>Registers <see cref="GmCommands"/> and <see cref="BanList"/> as singletons.</summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The same service collection.</returns>
    public static IServiceCollection AddAdmin(this IServiceCollection services)
    {
        services.AddSingleton<GmCommands>();
        services.AddSingleton<BanList>();
        return services;
    }
}
