namespace NorthIsland.Server.Features.Items;

/// <summary>Settings for equipment wear.</summary>
public sealed class DurabilityOptions
{
    /// <summary>The wear added to each worn piece after a battle that wears gear; 0 switches wear off.</summary>
    public int WearPerBattle { get; set; }

    /// <summary>How many levels the highest monster must be above the player for a battle to wear gear; 0 or less makes every battle wear.</summary>
    public int WearMinLevelGap { get; set; } = 19;

    /// <summary>The wear a gun gets after every battle, whatever the level gap; 0 for none.</summary>
    public int GunWearPerBattle { get; set; }

    /// <summary>The wear the Attack Pestle gets after every battle, whatever the level gap; 0 for none.</summary>
    public int PestleWearPerBattle { get; set; }
}
