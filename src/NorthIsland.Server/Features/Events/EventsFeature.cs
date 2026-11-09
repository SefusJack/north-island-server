using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace NorthIsland.Server.Features.Events;

/// <summary>Registers the event services.</summary>
public static class EventsFeature
{
    /// <summary>Binds the event options and adds the event calendar, services, engine and handlers as singletons.</summary>
    /// <param name="services">The service collection to add to.</param>
    /// <param name="section">The configuration section of the event options.</param>
    /// <returns>The same service collection.</returns>
    public static IServiceCollection AddEvents(this IServiceCollection services, IConfiguration section)
    {
        services.Configure<EventOptions>(section);
        services.AddSingleton<NorthIsland.Server.Features.Events.EventCalendar>();
        services.AddSingleton<NorthIsland.Server.Features.Events.Services>();
        services.AddSingleton<NorthIsland.Server.Features.Events.EventEngine>();
        services.AddSingleton<EventHandlers>();
        return services;
    }
}
