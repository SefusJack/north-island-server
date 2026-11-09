using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace NorthIsland.Server.Features.Houses;

/// <summary>Background service that, once a second, runs the bath and training steps of the sessions that bathe or train.</summary>
/// <param name="handlers">The house life handlers that do the steps.</param>
/// <param name="logger">The logger.</param>
public sealed class HouseLifeTicker(
    HouseLifeHandlers handlers,
    ILogger<HouseLifeTicker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(1));
        try
        {
            while (await timer.WaitForNextTickAsync(stoppingToken).ConfigureAwait(false))
                foreach (var session in handlers.Online().ToList())
                    if ((session.HouseLife.BathFurn != 0 || session.HouseLife.Training is not null)
                        && session.ActiveCharacter is { } c
                        && session.State == SessionState.InWorld)
                    {
                        try { handlers.Tick(session, c, GameClock.UtcNow); }
                        catch (Exception ex) when (ex is not OperationCanceledException) { logger.LogError(ex, "house life tick failed"); }
                    }
        }
        catch (OperationCanceledException)
        {
        }
    }
}
