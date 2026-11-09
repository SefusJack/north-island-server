using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NorthIsland.Protocol;
using NorthIsland.Protocol.Messages.World;

namespace NorthIsland.Server.Features.Activities;

/// <summary>
/// Background service that checks the activity list every 20 seconds and pushes it, or only the changed states, to the 2014 clients online.
/// </summary>
/// <param name="activities">Builds the activity list.</param>
/// <param name="calendar">The event calendar giving the clock.</param>
/// <param name="world">The world state, used to find the online players.</param>
/// <param name="logger">Logger for the push message.</param>
public sealed class ActivityTicker(
    ActivityHandlers activities,
    EventCalendar calendar,
    WorldState world,
    ILogger<ActivityTicker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var last = activities.Entries(calendar.Clock());
        var (lastVersion, lastDay) = (ActivitySchedule.Version(last), ActivitySchedule.Weekday(calendar.Clock().DayOfWeek));
        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(20));
        try
        {
            while (await timer.WaitForNextTickAsync(stoppingToken).ConfigureAwait(false))
            {
                var now = calendar.Clock();
                var current = activities.Entries(now);
                var (version, day) = (ActivitySchedule.Version(current), ActivitySchedule.Weekday(now.DayOfWeek));
                byte[]? packet = null;
                if (version != lastVersion || day != lastDay)
                    packet = ActivityMessages.List(day, version, current);
                else
                {
                    List<(byte, byte)> changed = [];
                    for (var i = 0; i < current.Count; i++)
                        if (current[i].State != last[i].State)
                            changed.Add(((byte)(i + 1), current[i].State));
                    if (changed.Count > 0)
                        packet = ActivityMessages.State(changed);
                }
                (last, lastVersion, lastDay) = (current, version, day);
                if (packet is null)
                    continue;
                foreach (var session in world.OnlineSessions().Where(s => s.Client == ClientGeneration.Client2014))
                    session.Send(packet);
                logger.LogInformation("Activity list: states pushed to the 2014 clients");
            }
        }
        catch (OperationCanceledException)
        {
        }
    }
}
