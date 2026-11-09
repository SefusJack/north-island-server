namespace NorthIsland.Protocol.Opcodes;

/// <summary>Action id 84: the Top 100 window.</summary>
public static class Action84
{
    /// <summary>Action id of this packet group, 84.</summary>
    public const byte Code = 84;

    /// <summary>Client to server: asks for the Top 100 window; the server also sends it to show the window.</summary>
    public const byte Request = 1;

    /// <summary>Server to client: a Top 100 page.</summary>
    public const byte List = 2;
}
