using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace NorthIsland.Server.Features.Items;

/// <summary>Background service that every 20 seconds expires leases and timed item effects of online players in the world.</summary>
/// <param name="effects">The item effect handlers that do the expiring.</param>
/// <param name="logger">Logger.</param>
public sealed class ItemEffectTicker(
    ItemEffectHandlers effects,
    ILogger<ItemEffectTicker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(20));
        try
        {
            while (await timer.WaitForNextTickAsync(stoppingToken).ConfigureAwait(false))
                foreach (var session in effects.Online().ToList())
                    if (session.ActiveCharacter is { } character && session.State == SessionState.InWorld)
                    {
                        try { effects.Tick(session, character); }
                        catch (Exception ex) when (ex is not OperationCanceledException) { logger.LogError(ex, "timed effect tick failed"); }
                    }
        }
        catch (OperationCanceledException)
        {
        }
    }
}
