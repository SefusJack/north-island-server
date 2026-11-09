using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace NorthIsland.Server.Features.Vehicles;

/// <summary>Background service that, once a minute, applies fuel use and wear to every player who is riding a vehicle.</summary>
/// <param name="care">Applies the tick to one session.</param>
/// <param name="world">The online sessions.</param>
/// <param name="logger">Logger for the handler's messages.</param>
public sealed class VehicleTicker(
    VehicleCareHandlers care,
    WorldState world,
    ILogger<VehicleTicker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromMinutes(1));
        try
        {
            while (await timer.WaitForNextTickAsync(stoppingToken).ConfigureAwait(false))
                foreach (var session in world.OnlineSessions().Where(s => s.Vehicle is { Riding: true }).ToList())
                {
                    try { care.Tick(session); }
                    catch (Exception ex) when (ex is not OperationCanceledException) { logger.LogError(ex, "vehicle tick failed"); }
                }
        }
        catch (OperationCanceledException)
        {
        }
    }
}
