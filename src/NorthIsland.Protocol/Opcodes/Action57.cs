namespace NorthIsland.Protocol.Opcodes;

/// <summary>Action id 57: sport minigames and carnival games (Lucky Cards, Lucky Wheel, Q&amp;A, Five in a Row, Golden Compass).</summary>
public static class Action57
{
    /// <summary>Action id of this packet group, 57.</summary>
    public const byte Code = 57;

    /// <summary>Server to client: starts a sport game.</summary>
    public const byte Start = 1;

    /// <summary>Client to server: the result of a sport game (1 won, 0 lost or quit).</summary>
    public const byte Result = 1;

    /// <summary>Server to client: frees the player from a sport game.</summary>
    public const byte Free = 2;

    /// <summary>Server to client: the function is unavailable at present.</summary>
    public const byte Unavailable = 3;

    /// <summary>Sub code of the Lucky Cards game, both directions; the next byte is one of the <c>Poke*</c> codes.</summary>
    public const byte Poke = 9;

    /// <summary>
    /// Lucky Cards sub codes: leave (2, client to server), flip a card (3, both ways), turns done (4, both ways), start (6, server to client).
    /// </summary>
    public const byte PokeLeft = 2, PokeFlip = 3, PokeDone = 4, PokeStart = 6;

    /// <summary>Sub code of the Lucky Wheel, both directions.</summary>
    public const byte Wheel = 12;

    /// <summary>Lucky Wheel sub codes from the client: refresh (1), double (2), paid with points (3), paid with tokens (4).</summary>
    public const byte WheelRefresh = 1, WheelDouble = 2, WheelPaidPoints = 3, WheelPaidTokens = 4;

    /// <summary>Lucky Wheel sub codes to the client: the wheel list (1) and the result of a double (2).</summary>
    public const byte WheelList = 1, WheelDoubled = 2;

    /// <summary>Sub code of the Q&amp;A activity, both directions.</summary>
    public const byte Quiz = 8;

    /// <summary>Client to server: the player leaves the Q&amp;A activity.</summary>
    public const byte QuizLeave = 2;

    /// <summary>Client to server: the player's answer in the Q&amp;A activity.</summary>
    public const byte QuizAnswer = 3;

    /// <summary>Sub code of Five in a Row, both directions.</summary>
    public const byte Gobang = 11;

    /// <summary>Five in a Row sub codes from the client: enter (1), quit (2), sit (3), start (4), move (5), take back (6).</summary>
    public const byte GobangEnter = 1, GobangQuit = 2, GobangSit = 3, GobangStart = 4, GobangMove = 5, GobangBack = 6;

    /// <summary>
    /// Five in a Row sub codes to the client: player list (1), table ante (2), seat taken (3), seat left (4), game started (5), play (6), a second
    /// ante code (7).
    /// </summary>
    public const byte GobangPlayers = 1, GobangAnte = 2, GobangSeat = 3, GobangSeatLeft = 4, GobangStarted = 5, GobangPlay = 6, GobangAnte2 = 7;

    /// <summary>Sub code of the Golden Compass game, both directions.</summary>
    public const byte Compass = 10;
}
