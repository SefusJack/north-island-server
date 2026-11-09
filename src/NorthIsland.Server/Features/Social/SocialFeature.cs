using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace NorthIsland.Server.Features.Social;

/// <summary>Registration of the social feature (options, friends, team, puppet, block list and wish items).</summary>
public static class SocialFeature
{
    /// <summary>Registers the social options and handlers.</summary>
    /// <param name="services">The service collection.</param>
    /// <param name="section">The configuration section for <see cref="SocialOptions"/>.</param>
    /// <returns>The same service collection.</returns>
    public static IServiceCollection AddSocial(this IServiceCollection services, IConfiguration section)
    {
        services.Configure<SocialOptions>(section);
        services.AddSingleton<SocialHandlers>();
        services.AddSingleton<FriendHandlers>();
        services.AddSingleton<TeamHandlers>();
        services.AddSingleton<PuppetHandlers>();
        services.AddSingleton<BlocklistHandlers>();
        services.AddSingleton<WishItemHandlers>();
        return services;
    }
}
