namespace NorthIsland.Protocol.Opcodes;

/// <summary>Action 10: nickname and profile.</summary>
public static class Action10
{
    /// <summary>Action code 10.</summary>
    public const byte Code = 10;

    /// <summary>Client sets its nickname; the server sends the nickname of a scene player.</summary>
    public const byte Nickname = 1;

    /// <summary>Client sets its profile; the server sends the heart number of a scene player.</summary>
    public const byte Profile = 2;

    /// <summary>Client to server: the profile form was submitted; the server only accepts it.</summary>
    public const byte Submitted = 3;
}
