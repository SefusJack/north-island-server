namespace NorthIsland.Server.Features.Events;

/// <summary>One weekly opening window of an activity, as listed in the calendar file.</summary>
public sealed class ActivityHours
{
    /// <summary>The activity this window belongs to, for example <see cref="EventCalendar.ActivityQuiz"/>; compared without regard to case.</summary>
    public string Name { get; set; } = "";

    /// <summary>The day of the week the window falls on.</summary>
    public DayOfWeek Day { get; set; }

    /// <summary>The local time the window opens, as <c>h:mm</c>.</summary>
    public string Start { get; set; } = "";

    /// <summary>The local time the window closes, as <c>h:mm</c>; the activity is shut from this minute on.</summary>
    public string End { get; set; } = "";
}
