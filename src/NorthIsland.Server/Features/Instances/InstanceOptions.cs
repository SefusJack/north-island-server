namespace NorthIsland.Server.Features.Instances;

/// <summary>The options of the instances and of the random fights in mines.</summary>
public sealed class InstanceOptions
{
    /// <summary>The chance in percent that a step in a mine area of an instance starts a fight; 0 or less switches the fights off.</summary>
    public int MineEncounterPercent { get; set; } = 15;

    /// <summary>The seconds after a mine fight (or the arrival) during which no new mine fight starts.</summary>
    public int MineEncounterCooldownSeconds { get; set; } = 20;

    /// <summary>Whether the level a scene needs is checked when creating or joining an instance.</summary>
    public bool InstanceLevelCheck { get; set; } = true;

    /// <summary>The seconds after creation before a waiting instance starts by itself, when a member is online; a full instance starts at once.</summary>
    public int InstanceStartSeconds { get; set; } = 60;
}
