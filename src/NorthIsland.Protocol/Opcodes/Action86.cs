namespace NorthIsland.Protocol.Opcodes;

/// <summary>Action id 86: the Lost Property Office.</summary>
public static class Action86
{
    /// <summary>Action id of this packet group, 86.</summary>
    public const byte Code = 86;

    /// <summary>Server to client: the lost property records.</summary>
    public const byte List = 1;

    /// <summary>Client to server: reclaims lost items; server to client: an item was given back.</summary>
    public const byte Reclaim = 2;

    /// <summary>Server to client: the reclaim was refused, with a reason code.</summary>
    public const byte Refused = 3;

    /// <summary>The number 100; no code uses it.</summary>
    public const int PointsEach = 100;
}
