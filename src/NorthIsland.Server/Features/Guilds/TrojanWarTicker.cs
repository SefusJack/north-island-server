using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace NorthIsland.Server.Features.Guilds;

/// <summary>
/// Background service that checks the war schedule every 10 seconds, sets the castle's war flag and sends the war messages to everybody online.
/// </summary>
/// <param name="war">The schedule.</param>
/// <param name="castles">The castle registry whose war flag is set.</param>
/// <param name="world">The world state, to find the online sessions.</param>
/// <param name="logger">The logger.</param>
public sealed class TrojanWarTicker(
    TrojanWar war,
    CastleRegistry castles,
    WorldState world,
    ILogger<TrojanWarTicker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        bool warned = false, warnedEnd = false;
        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(10));
        try
        {
            do
            {
                (var on, var messages, warned, warnedEnd) = war.Phase(GameClock.Now, castles.WarOn, warned, warnedEnd);
                castles.WarOn = on;
                foreach (var message in messages)
                {
                    logger.LogInformation("Trojan War: message {Message}", message);
                    var packet = TrojanWar.Message(message);
                    foreach (var session in world.OnlineSessions())
                        session.Send(packet);
                }
            }
            while (await timer.WaitForNextTickAsync(stoppingToken).ConfigureAwait(false));
        }
        catch (OperationCanceledException)
        {
        }
    }
}
