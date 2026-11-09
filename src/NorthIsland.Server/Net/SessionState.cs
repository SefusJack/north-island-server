namespace NorthIsland.Server.Net;

/// <summary>How far a session has got.</summary>
public enum SessionState
{
    /// <summary>The connection is open and no server has been selected.</summary>
    Connected,

    /// <summary>A server was selected and the session waits for the login.</summary>
    AwaitLogin,

    /// <summary>The player is logged in and sees the character list.</summary>
    CharacterSelect,

    /// <summary>The player is creating a character.</summary>
    Creating,

    /// <summary>The player is in the game world.</summary>
    InWorld,
}
