namespace NorthIsland.Protocol.Opcodes;

/// <summary>Sub codes of action 9, character creation and the name check.</summary>
public static class Action9
{
    /// <summary>The action number, 9.</summary>
    public const byte Code = 9;

    /// <summary>Client to server: create a character.</summary>
    public const byte Create = 1;

    /// <summary>Client to server: ask whether a character name can be used.</summary>
    public const byte CheckName = 2;

    /// <summary>Server to client: the result of a name check.</summary>
    public const byte NameResult = 3;
}
