namespace NorthIsland.Protocol.Messages.Events;

/// <summary>Codes of the 85/3 packet: the answer to joining an instance.</summary>
public static class InstanceJoinCodes
{
    /// <summary>The player joined.</summary>
    public const byte Joined = 0;

    /// <summary>The instance is full.</summary>
    public const byte Full = 1;

    /// <summary>The player's level does not fit.</summary>
    public const byte Level = 2;

    /// <summary>The instance has already started.</summary>
    public const byte Started = 3;

    /// <summary>The instance was cancelled.</summary>
    public const byte Cancelled = 4;

    /// <summary>The player already used the instance today.</summary>
    public const byte DoneToday = 5;

    /// <summary>The instance has ended.</summary>
    public const byte Ended = 6;

    /// <summary>The instance belongs to a guild the player is not in.</summary>
    public const byte Guild = 7;

    /// <summary>The join failed.</summary>
    public const byte Failed = 255;
}
