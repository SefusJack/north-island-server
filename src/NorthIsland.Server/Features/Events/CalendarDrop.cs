namespace NorthIsland.Server.Features.Events;

/// <summary>A row of a seasonal event's drop table.</summary>
public sealed class CalendarDrop
{
    /// <summary>The item id that drops.</summary>
    public ushort Item { get; set; }

    /// <summary>The row's weight in percent in the drop roll.</summary>
    public double Percent { get; set; }
}
