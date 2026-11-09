namespace NorthIsland.Server.Features.Events;

/// <summary>Settings of the New Year event.</summary>
public sealed class NewYearOptions
{
    /// <summary>The name of the calendar event that is the New Year event; empty turns its notices off.</summary>
    public string EventName { get; set; } = "New Year";

    /// <summary>How many Lucky 7 plays in the event make the next one free.</summary>
    public int FreeAfter { get; set; } = 7;
}
