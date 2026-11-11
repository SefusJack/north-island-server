namespace NorthIsland.Protocol.Opcodes;

/// <summary>Sub codes of action <c>35</c>: account actions: password change, character deletion and IM points.</summary>
public static class Action35
{
    /// <summary>The action byte <c>35</c> that starts every packet of this group.</summary>
    public const byte Code = 35;

    /// <summary>Both ways: changes the active character's secret code; the server answers with a result code.</summary>
    public const byte ChangePassword = 1;

    /// <summary>
    /// Both ways: changes the account password (north-island-client only, not in the original client); the server answers with a result code.
    /// </summary>
    public const byte ChangeAccountPassword = 12;

    /// <summary>Both ways: deletes a character; the server answers with a result and the slot.</summary>
    public const byte Delete = 2;

    /// <summary>Server to client: the IM points and bonus points.</summary>
    public const byte Points = 4;
}
