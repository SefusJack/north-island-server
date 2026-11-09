namespace NorthIsland.Server.Features.Guilds;

/// <summary>The Trojan War schedule.</summary>
public sealed class TrojanWarOptions
{
    /// <summary>Whether the war runs.</summary>
    public bool Enabled { get; set; }

    /// <summary>The day of the week of the war.</summary>
    public DayOfWeek Day { get; set; } = DayOfWeek.Sunday;

    /// <summary>The hour the war starts.</summary>
    public int StartHour { get; set; } = 19;

    /// <summary>The hour the war ends.</summary>
    public int EndHour { get; set; } = 21;

    /// <summary>Minutes before the start and the end at which the warning is sent.</summary>
    public int WarnMinutes { get; set; } = 5;
}
