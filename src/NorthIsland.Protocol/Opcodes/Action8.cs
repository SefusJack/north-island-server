namespace NorthIsland.Protocol.Opcodes;

/// <summary>Action id 8: attribute changes of the player, pets and team mates, and spending of stat points.</summary>
public static class Action8
{
    /// <summary>Action id of this packet group, 8.</summary>
    public const byte Code = 8;

    /// <summary>Server to client: sets one attribute of the player.</summary>
    public const byte SetAttribute = 1;

    /// <summary>Server to client: an attribute of a team mate.</summary>
    public const byte WorldPlayerStat = 3;

    /// <summary>Server to client: an attribute of a pet.</summary>
    public const byte PetAttribute = 2;

    /// <summary>Client to server: spends stat points (same value as <c>SetAttribute</c>).</summary>
    public const byte SpendStatPoints = 1;

    /// <summary>Attribute id of intelligence.</summary>
    public const byte AttrInt = 27;

    /// <summary>Attribute id of strength.</summary>
    public const byte AttrStr = 28;

    /// <summary>Attribute id of constitution.</summary>
    public const byte AttrCon = 29;

    /// <summary>Attribute id of agility.</summary>
    public const byte AttrAgi = 30;

    /// <summary>Attribute id of wisdom.</summary>
    public const byte AttrWis = 33;

    /// <summary>Attribute id of the current HP.</summary>
    public const byte AttrHp = 25;

    /// <summary>Attribute id of the current SP.</summary>
    public const byte AttrSp = 26;

    /// <summary>Attribute id of the level.</summary>
    public const byte AttrLevel = 35;

    /// <summary>Attribute id of the experience.</summary>
    public const byte AttrExp = 36;

    /// <summary>Attribute id of the unspent skill points.</summary>
    public const byte AttrSkillPoints = 37;

    /// <summary>Attribute id of the unspent stat points.</summary>
    public const byte AttrStatPoints = 38;

    /// <summary>Attribute id of the job.</summary>
    public const byte AttrJob = 31;

    /// <summary>Attribute id of the reborn count.</summary>
    public const byte AttrReborn = 39;

    /// <summary>Attribute id of a pet's amity.</summary>
    public const byte AttrAmity = 64;

    /// <summary>Attribute id of a skill's level; the second value is the skill id.</summary>
    public const byte AttrSkillLevel = 110;

    /// <summary>Attribute id of a skill's experience; the second value is the skill id.</summary>
    public const byte AttrSkillExp = 111;

    /// <summary>Attribute id of the maximum HP.</summary>
    public const byte AttrMaxHp = 205;

    /// <summary>Attribute id of the maximum SP.</summary>
    public const byte AttrMaxSp = 206;

    /// <summary>Attribute id of the extra maximum HP.</summary>
    public const byte AttrMaxHpExtra = 48;

    /// <summary>Attribute id of the extra maximum SP.</summary>
    public const byte AttrMaxSpExtra = 49;
}
