using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace NorthIsland.Server.Features.Items;

/// <summary>Background service that once a second ends the body shifts of online players whose time ran out.</summary>
/// <param name="handlers">The body shift handlers that do the ending.</param>
/// <param name="logger">Logger.</param>
public sealed class BodyShiftTicker(
    BodyShiftHandlers handlers,
    ILogger<BodyShiftTicker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(1));
        try
        {
            while (await timer.WaitForNextTickAsync(stoppingToken).ConfigureAwait(false))
                foreach (var session in handlers.Online().ToList())
                    if (session.ActiveCharacter is { BodyShiftOn: true } c && session.State == SessionState.InWorld)
                    {
                        try { handlers.Tick(session, c, GameClock.UtcNow); }
                        catch (Exception ex) when (ex is not OperationCanceledException) { logger.LogError(ex, "body shift tick failed"); }
                    }
        }
        catch (OperationCanceledException)
        {
        }
    }
}
