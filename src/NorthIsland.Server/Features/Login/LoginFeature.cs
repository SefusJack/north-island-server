using Microsoft.Extensions.DependencyInjection;

namespace NorthIsland.Server.Features.Login;

/// <summary>Registration of the login and world-entry services.</summary>
public static class LoginFeature
{
    /// <summary>Adds the login handlers and the world entry as singletons.</summary>
    /// <param name="services">The service collection to add to.</param>
    /// <returns>The same collection, for chaining.</returns>
    public static IServiceCollection AddLogin(this IServiceCollection services)
    {
        services.AddSingleton<LoginHandlers>();
        services.AddSingleton<WorldEntry>();
        return services;
    }
}
