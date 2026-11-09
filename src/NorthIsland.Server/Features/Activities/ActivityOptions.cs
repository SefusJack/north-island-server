namespace NorthIsland.Server.Features.Activities;

/// <summary>Configuration of the activities feature.</summary>
public sealed class ActivityOptions
{
    /// <summary>Settings of the online draw.</summary>
    public OnlineDrawOptions OnlineDraw { get; set; } = new();

    /// <summary>Settings of the hot springs.</summary>
    public HotSpringOptions HotSpring { get; set; } = new();

    /// <summary>When true every weekly activity is shown as running during its whole day, outside its hours too.</summary>
    public bool ActivitiesAlwaysOpen { get; set; }

    /// <summary>Seconds between two fishing attempts.</summary>
    public int FishingSeconds { get; set; } = 60;

    /// <summary>Percent chance that an attempt catches something.</summary>
    public int FishingCatchPercent { get; set; } = 50;
}
