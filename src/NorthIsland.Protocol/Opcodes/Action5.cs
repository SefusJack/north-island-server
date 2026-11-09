namespace NorthIsland.Protocol.Opcodes;

/// <summary>
/// Action id 5: the player's own state and look: bookcase and desk, equipment changes, attributes, level, job and time-effect icons.
/// </summary>
public static class Action5
{
    /// <summary>Action id of this packet group, 5.</summary>
    public const byte Code = 5;

    /// <summary>Client to server: puts a bag item into a bookcase slot.</summary>
    public const byte BookcasePut = 9;

    /// <summary>Client to server: takes the item out of a bookcase slot.</summary>
    public const byte BookcaseTake = 10;

    /// <summary>Client to server: puts a bag item into a desk slot.</summary>
    public const byte DeskPut = 11;

    /// <summary>Client to server: takes the item out of a desk slot.</summary>
    public const byte DeskTake = 12;

    /// <summary>Server to client: the content of one bookcase slot.</summary>
    public const byte BookcaseSlot = 13;

    /// <summary>Server to client: the content of one desk slot.</summary>
    public const byte DeskSlot = 24;

    /// <summary>Client to server: the player leaves the automatic play (AFK) mode.</summary>
    public const byte AfkStop = 8;

    /// <summary>Server to client: players that look like an NPC, or like themselves again.</summary>
    public const byte Transform = 5;

    /// <summary>Server to client: another player took an item off.</summary>
    public const byte EquipOff = 1;

    /// <summary>Server to client: tells the client that a return spot is recorded.</summary>
    public const byte ReturnSpotRecorded = 21;

    /// <summary>Client to server: the return spot the player chose.</summary>
    public const byte ReturnSpot = 17;

    /// <summary>Server to client: another player put an item on.</summary>
    public const byte EquipOn = 2;

    /// <summary>Server to client: the player's own attribute block.</summary>
    public const byte OwnAttributes = 3;

    /// <summary>Server to client: a player's role (body type and head).</summary>
    public const byte RoleChange = 12;

    /// <summary>Server to client: tells the client the scene is ready so that players are drawn.</summary>
    public const byte SceneReady = 4;

    /// <summary>Server to client: the new level of another player in the scene.</summary>
    public const byte SceneLevel = 18;

    /// <summary>Server to client: shows the level-up text over a player.</summary>
    public const byte LevelUp = 10;

    /// <summary>Server to client: the online status of another player.</summary>
    public const byte StatusChanged = 8;

    /// <summary>Client to server: the player's own online status.</summary>
    public const byte OnlineStatus = 7;

    /// <summary>Server to client: the job of a player, the own player included.</summary>
    public const byte PlayerJob = 23;

    /// <summary>Server to client: the double experience icon on or off; no code sends it.</summary>
    public const byte DoubleExp = 14;

    /// <summary>Server to client: state of the Lucky Grass time effect.</summary>
    public const byte LuckyGrass = 15;

    /// <summary>Server to client: state of the Spa Bun time effect.</summary>
    public const byte SpaBun = 16;

    /// <summary>Client to server: the Lucky Grass icon was clicked (same value as <c>LuckyGrass</c>).</summary>
    public const byte LuckyGrassClick = 15;

    /// <summary>Client to server: the Spa Bun icon was clicked (same value as <c>SpaBun</c>).</summary>
    public const byte SpaBunClick = 16;

    /// <summary>Server to client: state of the experience potion icon.</summary>
    public const byte ExpPotion = 17;

    /// <summary>Client to server: the experience icon was clicked, asking for the time left.</summary>
    public const byte ExpClick = 14;
}
