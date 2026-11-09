using Microsoft.Extensions.Hosting;

namespace NorthIsland.Server.World;

/// <summary>A background service that ticks the ground items once a second so dropped things expire and map things come back.</summary>
/// <param name="ground">The ground items to tick.</param>
public sealed class GroundRespawn(
    GroundItems ground) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(1));
        try
        {
            while (await timer.WaitForNextTickAsync(stoppingToken).ConfigureAwait(false))
                ground.Tick();
        }
        catch (OperationCanceledException)
        {
        }
    }
}
