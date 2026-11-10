namespace NorthIsland.Server.Features.Instances;

/// <summary>The options of the instances.</summary>
public sealed class InstanceOptions
{
    /// <summary>Whether the level a scene needs is checked when creating or joining an instance.</summary>
    public bool InstanceLevelCheck { get; set; } = true;

    /// <summary>The seconds after creation before a waiting instance starts by itself, when a member is online; a full instance starts at once.</summary>
    public int InstanceStartSeconds { get; set; } = 60;
}
