namespace NorthIsland.Server.World;

/// <summary>The settings of roaming NPCs, ground items and world chat.</summary>
public sealed class WorldOptions
{
    /// <summary>The shortest pause between two wander walks of a roaming NPC, in seconds.</summary>
    public int WanderMinSeconds { get; set; } = 3;

    /// <summary>The longest pause between two wander walks, in seconds; 0 or less turns roaming off.</summary>
    public int WanderMaxSeconds { get; set; } = 8;

    /// <summary>How far a roaming NPC notices a player to chase, in pixels; 0 or less turns chasing off.</summary>
    public int ChaseRadius { get; set; } = 160;

    /// <summary>How close a chasing NPC must get to touch a player and start its event, in pixels.</summary>
    public int TouchRadius { get; set; } = 24;

    /// <summary>The seconds after arriving on a map during which a player is not chased.</summary>
    public int ArrivalGraceSeconds { get; set; } = 8;

    /// <summary>The seconds until a taken map item is back on the ground.</summary>
    public int GroundItemRespawnSeconds { get; set; } = 60;

    /// <summary>The seconds a dropped item stays on the ground.</summary>
    public int DroppedItemSeconds { get; set; } = 300;

    /// <summary>How far in pixels, on each axis, a dropped item or gold can land from the player; 0 drops it at their feet.</summary>
    public int DropScatterPixels { get; set; }

    /// <summary>The SP a world chat message costs a player wearing the Radio Set.</summary>
    public int WorldChatSp { get; set; } = 15;
}
