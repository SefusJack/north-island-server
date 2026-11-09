namespace NorthIsland.Protocol.Opcodes;

/// <summary>Action id 70: the castle war.</summary>
public static class Action70
{
    /// <summary>Action id of this packet group, 70.</summary>
    public const byte Code = 70;

    /// <summary>Sub code of the castle war messages; the next byte is the message number.</summary>
    public const byte War = 1;

    /// <summary>Server to client: war message 10, the new castle leader.</summary>
    public const byte NewLeader = 10;

    /// <summary>Server to client: war message 23, the guild that owns the castle.</summary>
    public const byte CastleOwner = 23;
}
