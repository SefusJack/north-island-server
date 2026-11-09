namespace NorthIsland.Server.Features.Events;

/// <summary>Names the map events whose date conditions follow a calendar event.</summary>
public sealed class CalendarScript
{
    /// <summary>The map id.</summary>
    public int Map { get; set; }

    /// <summary>The event indexes on the map; only their date conditions are affected.</summary>
    public List<int> Events { get; set; } = [];
}
