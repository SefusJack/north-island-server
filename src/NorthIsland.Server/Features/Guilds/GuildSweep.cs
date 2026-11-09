using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace NorthIsland.Server.Features.Guilds;

/// <summary>Background service that runs the guild dismissal check every <see cref="GuildOptions.SweepMinutes"/> minutes.</summary>
/// <param name="guilds">The guild handlers that do the check.</param>
/// <param name="options">The guild feature options.</param>
public sealed class GuildSweep(
    GuildHandlers guilds,
    IOptions<GuildFeatureOptions> options) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var minutes = options.Value.Guilds.SweepMinutes;
        if (minutes <= 0)
            return;
        using var timer = new PeriodicTimer(TimeSpan.FromMinutes(minutes));
        try
        {
            do
                guilds.Sweep(GameClock.Now);
            while (await timer.WaitForNextTickAsync(stoppingToken).ConfigureAwait(false));
        }
        catch (OperationCanceledException)
        {
        }
    }
}
