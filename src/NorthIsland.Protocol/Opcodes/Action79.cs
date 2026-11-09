namespace NorthIsland.Protocol.Opcodes;

/// <summary>Action id 79: choosing a career (rebirth).</summary>
public static class Action79
{
    /// <summary>Action id of this packet group, 79.</summary>
    public const byte Code = 79;

    /// <summary>Client to server: chooses a career; server to client: shows the career form.</summary>
    public const byte Choose = 1;

    /// <summary>Client to server: changes the career (same value as <c>Result</c>).</summary>
    public const byte Change = 2;

    /// <summary>Server to client: the result code of a career step (same value as <c>Change</c>).</summary>
    public const byte Result = 2;
}
