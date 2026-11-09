using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NorthIsland.Protocol.Messages.Player;

namespace NorthIsland.Server.Features.Events;

/// <summary>
/// Background service that checks the calendar every minute, sends the New Year and event notices to the players online when events start or end,
/// and logs which seasonal events are on.
/// </summary>
/// <param name="calendar">The event calendar.</param>
/// <param name="world">The world state, for the players online.</param>
/// <param name="logger">The log.</param>
public sealed class CalendarTicker(
    EventCalendar calendar,
    WorldState world,
    ILogger<CalendarTicker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var newYear = calendar.NewYearOn();
        var on = calendar.Active();
        var active = string.Join(", ", on.Select(e => e.Name));
        logger.LogInformation("Seasonal events on: {Events}", active.Length > 0 ? active : "none");
        using var timer = new PeriodicTimer(TimeSpan.FromMinutes(1));
        try
        {
            while (await timer.WaitForNextTickAsync(stoppingToken).ConfigureAwait(false))
            {
                if (calendar.NewYearOn() != newYear)
                {
                    newYear = !newYear;
                    foreach (var session in world.OnlineSessions().Where(s => s.ActiveCharacter is not null))
                    {
                        session.ActiveCharacter!.NewYearLucky7 = 0;
                        session.Send(PlayerMessages.NewYearNotice(newYear));
                        if (newYear)
                            session.Send(PlayerMessages.Lucky7Count(0));
                    }
                }
                var nowOn = calendar.Active();
                var notices = EventCalendar.NoticesBetween(on, nowOn);
                on = nowOn;
                if (notices.Count > 0)
                    foreach (var session in world.OnlineSessions().Where(s => s.ActiveCharacter is not null))
                        foreach (var notice in notices)
                            session.Send(notice);
                var names = string.Join(", ", nowOn.Select(e => e.Name));
                if (names != active)
                    logger.LogInformation("Seasonal events on: {Events}", names.Length > 0 ? names : "none");
                active = names;
            }
        }
        catch (OperationCanceledException)
        {
        }
    }
}
