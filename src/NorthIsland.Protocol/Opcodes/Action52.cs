namespace NorthIsland.Protocol.Opcodes;

/// <summary>Action id 52: round phases of a battle.</summary>
public static class Action52
{
    /// <summary>Action id of this packet group, 52.</summary>
    public const byte Code = 52;

    /// <summary>Server to client: starts the command phase of a round.</summary>
    public const byte CommandPhase = 1;
}
