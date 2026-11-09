using NorthIsland.Protocol.Messages.Player;

namespace NorthIsland.Server.Features.Events;

/// <summary>One seasonal event of the calendar file: when it is on and what it changes while it is.</summary>
public sealed class CalendarEvent
{
    /// <summary>The event's name; the New Year event is found by it.</summary>
    public string Name { get; set; } = "";

    /// <summary>Whether the event follows its dates, is always on or is off.</summary>
    public CalendarMode Mode { get; set; } = CalendarMode.Auto;

    /// <summary>The first day of the yearly window as <c>MM-dd</c>, local time; null for none.</summary>
    public string? Start { get; set; }

    /// <summary>The last day of the yearly window as <c>MM-dd</c>, included; it may lie in the next year; null for none.</summary>
    public string? End { get; set; }

    /// <summary>
    /// Windows for single years, by year text: the first and last day as <c>MM-dd</c>.
    /// A listed year replaces <see cref="Start"/> and <see cref="End"/>.
    /// </summary>
    public Dictionary<string, List<string>> Years { get; set; } = [];

    /// <summary>The days of the week the event is on, whole days, local time; when any are listed the dates are not used.</summary>
    public List<DayOfWeek> Days { get; set; } = [];

    /// <summary>The factor battle experience is multiplied by while the event is on.</summary>
    public double ExpMultiplier { get; set; } = 1;

    /// <summary>The extra drop table of the event, rolled for a defeated monster while the event is on.</summary>
    public List<CalendarDrop> Drops { get; set; } = [];

    /// <summary>The id of the event notice sent when the event starts and at login while it is on; 0 for none.</summary>
    public byte StartNotice { get; set; }

    /// <summary>The id of the event notice sent when the event ends; 0 for none.</summary>
    public byte EndNotice { get; set; }

    /// <summary>The map scripts whose date conditions follow this event instead of the real date.</summary>
    public List<CalendarScript> Scripts { get; set; } = [];
}
