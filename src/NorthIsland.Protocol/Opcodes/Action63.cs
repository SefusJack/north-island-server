namespace NorthIsland.Protocol.Opcodes;

/// <summary>Action id 63: account login and character selection.</summary>
public static class Action63
{
    /// <summary>Action id of this packet group, 63.</summary>
    public const byte Code = 63;

    /// <summary>Client to server: back to the login screen.</summary>
    public const byte BackToLogin = 0;

    /// <summary>Client to server: login with a numeric id (same value as <c>CharacterList</c>).</summary>
    public const byte NumericLogin = 1;

    /// <summary>Server to client: the character list (same value as <c>NumericLogin</c>).</summary>
    public const byte CharacterList = 1;

    /// <summary>Client to server: selects a character slot (same value as <c>LoginId</c>).</summary>
    public const byte SelectSlot = 2;

    /// <summary>Server to client: the login id of the account (same value as <c>SelectSlot</c>).</summary>
    public const byte LoginId = 2;

    /// <summary>Client to server: cancels the character creation.</summary>
    public const byte CancelCreation = 3;

    /// <summary>Client to server: login with an account name and password.</summary>
    public const byte AccountLogin = 4;
}
