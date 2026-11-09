namespace NorthIsland.Protocol.Opcodes;

/// <summary>Action 1: login results and player-left notices from the server.</summary>
public static class Action1
{
    /// <summary>Action code 1.</summary>
    public const byte Code = 1;

    /// <summary>Server to client: a player left the scene.</summary>
    public const byte PlayerLeft = 1;

    /// <summary>Server to client: opens the character creation wizard.</summary>
    public const byte StartCharacterCreation = 3;

    /// <summary>Server to client: login rejected with a time-out message.</summary>
    public const byte TimeOut = 5;

    /// <summary>Server to client: login rejected because of a wrong account or password.</summary>
    public const byte WrongAccountOrPassword = 6;

    /// <summary>Server to client: login rejected with a connection failed message.</summary>
    public const byte ConnectionFailed = 7;

    /// <summary>Server to client: the character is locked; carries the seconds remaining.</summary>
    public const byte CharacterLocked = 8;

    /// <summary>Server to client: the selected server is accepted; carries its id and name.</summary>
    public const byte ServerSelected = 9;

    /// <summary>Server to client: the account is locked; carries the seconds remaining.</summary>
    public const byte AccountLocked = 10;

    /// <summary>Server to client, 2014 client only: the enter-game sequence is complete.</summary>
    public const byte EnterGameDone = 11;
}
