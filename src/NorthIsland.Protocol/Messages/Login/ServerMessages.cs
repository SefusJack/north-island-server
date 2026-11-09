using NorthIsland.Protocol.IO;
using NorthIsland.Protocol.Opcodes;

namespace NorthIsland.Protocol.Messages.Login;

/// <summary>Builders for the login and character selection packets, all server to client.</summary>
public static class ServerMessages
{
    /// <summary>Tells the client which server it is on.</summary>
    /// <param name="serverId">Id of the server.</param>
    /// <param name="flag">A flag byte.</param>
    /// <param name="serverName">Name of the server.</param>
    /// <returns>The packet.</returns>
    public static byte[] ServerSelected(ushort serverId, bool flag, string serverName) =>
        new PacketWriter(Action1.Code, Action1.ServerSelected).U16(serverId).Bool(flag).RawString(serverName).ToArray();

    /// <summary>Sends the client back to the login form with a message.</summary>
    /// <param name="sub">Sub code <c>1/5</c>, <c>1/6</c> or <c>1/7</c>.</param>
    /// <returns>The packet.</returns>
    public static byte[] LoginRejected(byte sub) =>
        sub is Action1.TimeOut or Action1.WrongAccountOrPassword or Action1.ConnectionFailed
            ? new PacketWriter(Action1.Code, sub).ToArray()
            : throw new ArgumentOutOfRangeException(nameof(sub), sub, "Expected 1/5, 1/6 or 1/7.");

    /// <summary>Shows the locked dialog.</summary>
    /// <param name="account">True for a locked account (<c>1/10</c>), false for a locked character (<c>1/8</c>).</param>
    /// <param name="secondsRemaining">Seconds until the lock ends.</param>
    /// <returns>The packet.</returns>
    public static byte[] Locked(bool account, int secondsRemaining) =>
        new PacketWriter(Action1.Code, account ? Action1.AccountLocked : Action1.CharacterLocked).I32(secondsRemaining).ToArray();

    /// <summary>Opens the character creation wizard.</summary>
    /// <param name="skipCipher">True to skip the cipher step.</param>
    /// <returns>The packet.</returns>
    public static byte[] StartCharacterCreation(bool skipCipher) =>
        new PacketWriter(Action1.Code, Action1.StartCharacterCreation).Bool(skipCipher).ToArray();

    /// <summary>Tells the 2014 client that entering the game is done.</summary>
    /// <returns>The packet.</returns>
    public static byte[] EnterGameDone() => new PacketWriter(Action1.Code, Action1.EnterGameDone).ToArray();

    /// <summary>Kicks the client with a reason.</summary>
    /// <param name="reason">The reason code.</param>
    /// <returns>The packet.</returns>
    public static byte[] Kick(byte reason) => new PacketWriter(Action0.Code, reason).ToArray();

    /// <summary>Sends the numeric login id.</summary>
    /// <param name="loginId">The login id.</param>
    /// <returns>The packet.</returns>
    public static byte[] LoginId(int loginId) =>
        new PacketWriter(Action63.Code, Action63.LoginId).I32(loginId).ToArray();

    /// <summary>Sends the account's character list.</summary>
    /// <param name="characters">The characters, in order.</param>
    /// <returns>The packet.</returns>
    public static byte[] CharacterList(IEnumerable<CharacterSummary> characters)
    {
        var w = new PacketWriter(Action63.Code, Action63.CharacterList);
        foreach (var c in characters)
            c.WriteTo(w);
        return w.ToArray();
    }

    /// <summary>Answers a character name check.</summary>
    /// <param name="result">The outcome.</param>
    /// <returns>The packet.</returns>
    public static byte[] NameCheckResult(NameCheck result) =>
        new PacketWriter(Action9.Code, Action9.NameResult).U8((byte)result).ToArray();

    /// <summary>Answers a character delete request.</summary>
    /// <param name="result">The result code.</param>
    /// <param name="slot">The slot of the character.</param>
    /// <returns>The packet.</returns>
    public static byte[] DeleteCharacterResult(byte result, byte slot) =>
        new PacketWriter(Action35.Code, Action35.Delete).U8(result).U8(slot).ToArray();

    /// <summary>Sends the player's own record, which enters the world.</summary>
    /// <param name="record">The record to send.</param>
    /// <returns>The packet.</returns>
    public static byte[] OwnPlayer(OwnPlayerRecord record)
    {
        var w = new PacketWriter(Action3.Code);
        record.WriteTo(w);
        return w.ToArray();
    }
}
