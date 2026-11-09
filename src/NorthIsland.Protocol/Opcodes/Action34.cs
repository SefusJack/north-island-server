namespace NorthIsland.Protocol.Opcodes;

/// <summary>Sub codes of action <c>34</c>: the IM points query.</summary>
public static class Action34
{
    /// <summary>The action byte <c>34</c> that starts every packet of this group.</summary>
    public const byte Code = 34;

    /// <summary>Client to server: asks for the player's IM points.</summary>
    public const byte AskPoints = 1;
}
