namespace NorthIsland.Protocol.Opcodes;

/// <summary>Sub codes of action <c>28</c>: skill learning.</summary>
public static class Action28
{
    /// <summary>The action byte <c>28</c> that starts every packet of this group.</summary>
    public const byte Code = 28;

    /// <summary>Client to server: learns a skill at the next level.</summary>
    public const byte Learn = 1;
}
