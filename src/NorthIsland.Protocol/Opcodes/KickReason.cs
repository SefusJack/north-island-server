namespace NorthIsland.Protocol.Opcodes;

/// <summary>The reason codes of the kick packet <c>0/reason</c> (server to client), after which the client disconnects.</summary>
public static class KickReason
{
    /// <summary>The server disconnected the player, for example by a GM command.</summary>
    public const byte Disconnected = 1;

    /// <summary>The login failed three times.</summary>
    public const byte LoginFailedThreeTimes = 3;

    /// <summary>The client's version number is not one the server accepts.</summary>
    public const byte FalseEdition = 9;

    /// <summary>The client's edition is out of date and must be updated.</summary>
    public const byte UpdateEdition = 17;

    /// <summary>The size of the client's Item.Dat differs from the size the server expects.</summary>
    public const byte InvalidItemVersion = 65;

    /// <summary>The account logged in again from another connection, which ends this one.</summary>
    public const byte RepeatedLogin = 19;
}
