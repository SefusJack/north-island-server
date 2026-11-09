namespace NorthIsland.Protocol.Opcodes;

/// <summary>Action id 71: results of the carnival machines; the second byte is the machine number.</summary>
public static class Action71
{
    /// <summary>Action id of this packet group, 71.</summary>
    public const byte Code = 71;

    /// <summary>Server to client: the player won; the body holds the prize, reels or stops and the count.</summary>
    public const byte Won = 1;

    /// <summary>Server to client: the player has no points left.</summary>
    public const byte NoPoints = 2;

    /// <summary>Server to client: a reward for the player.</summary>
    public const byte Reward = 3;

    /// <summary>Server to client: a reward for the player, and the game closes.</summary>
    public const byte RewardAndExit = 4;
}
