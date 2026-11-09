namespace NorthIsland.Protocol.Opcodes;

/// <summary>Action id 72: the house blackboard.</summary>
public static class Action72
{
    /// <summary>Action id of this packet group, 72.</summary>
    public const byte Code = 72;

    /// <summary>Client to server: opens the blackboard; server to client: the blackboard text.</summary>
    public const byte Open = 1;

    /// <summary>Client to server: writes on the blackboard.</summary>
    public const byte Write = 2;

    /// <summary>Client to server: erases the blackboard.</summary>
    public const byte Erase = 3;
}
