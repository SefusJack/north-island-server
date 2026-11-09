namespace NorthIsland.Server.Features.Events;

/// <summary>The contents of the calendar file: the seasonal events and the weekly activity hours.</summary>
public sealed class CalendarFile
{
    /// <summary>The seasonal events.</summary>
    public List<CalendarEvent> Events { get; set; } = [];

    /// <summary>The weekly opening windows of the activities; an activity with no row is always open.</summary>
    public List<ActivityHours> Activities { get; set; } = [];
}
