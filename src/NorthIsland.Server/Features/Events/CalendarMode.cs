namespace NorthIsland.Server.Features.Events;

/// <summary>How a calendar event decides whether it is on.</summary>
public enum CalendarMode
{
    /// <summary>The event is on inside its dated window.</summary>
    Auto,

    /// <summary>The event is always on.</summary>
    On,

    /// <summary>The event is never on.</summary>
    Off,
}
