namespace NorthIsland.Protocol.Opcodes;

/// <summary>Action id 6: walking.</summary>
public static class Action6
{
    /// <summary>Action id of this packet group, 6.</summary>
    public const byte Code = 6;

    /// <summary>Client to server: walk to a position; server to client: another player walks.</summary>
    public const byte WalkTo = 1;

    /// <summary>Client to server: the player stops walking.</summary>
    public const byte Stop = 2;
}
