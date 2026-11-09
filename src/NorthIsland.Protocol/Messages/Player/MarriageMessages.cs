using NorthIsland.Protocol.IO;
using NorthIsland.Protocol.Opcodes;
using NorthIsland.Protocol.Text;

namespace NorthIsland.Protocol.Messages.Player;

/// <summary>Builders and result codes for the marriage packets of action 45, all server to client.</summary>
public static class MarriageMessages
{
    /// <summary>Result code 1: the level is too low.</summary>
    public const byte LevelTooLow = 1;

    /// <summary>Result code 2: already married.</summary>
    public const byte AlreadyMarried = 2;

    /// <summary>Result code 3: an application was made before.</summary>
    public const byte AppliedBefore = 3;

    /// <summary>Result code 5: the application was cancelled.</summary>
    public const byte Cancelled = 5;

    /// <summary>Result code 6: not enough gold.</summary>
    public const byte NoMoney = 6;

    /// <summary>Result code 7: the fee was paid.</summary>
    public const byte FeePaid = 7;

    /// <summary>Result code 8: the application was registered; the deadline follows in a separate packet.</summary>
    public const byte Applied = 8;

    /// <summary>Result code 9: the two players are of the same gender.</summary>
    public const byte SameGender = 9;

    /// <summary>Result code 15: there is no application.</summary>
    public const byte NoApplication = 15;

    /// <summary>Result code 16: the ceremony needs exactly two players.</summary>
    public const byte NotTwo = 16;

    /// <summary>Result code 13: the groom lacks the wedding clothes or ring.</summary>
    public const byte GroomClothes = 13;

    /// <summary>Result code 18: no response.</summary>
    public const byte NoResponse = 18;

    /// <summary>Result code 20: the bride lacks the wedding clothes or ring.</summary>
    public const byte BrideClothes = 20;

    /// <summary>Result code 21: sets the wedding music.</summary>
    public const byte Music = 21;

    /// <summary>Result code 24: the divorce cooldown runs.</summary>
    public const byte DivorceCooldown = 24;

    /// <summary>Result code 25: a wedding is already running.</summary>
    public const byte WeddingRunning = 25;

    /// <summary>Result code 27: a ring is missing.</summary>
    public const byte NoRing = 27;

    /// <summary>Result code 28: the divorce fee was paid.</summary>
    public const byte DivorceFeePaid = 28;

    /// <summary>Result code 30: the application deadline.</summary>
    public const byte Deadline = 30;

    /// <summary>Result code 26: sent instead of 15 when some other application exists.</summary>
    public const byte MembersNotApplied = 26;

    /// <summary>Result code 32: refused because the body is shifted.</summary>
    public const byte BodyShifted = 32;

    /// <summary>Result code 29: the names of the couple for the ceremony line.</summary>
    public const byte CeremonyNames = 29;

    /// <summary>Result code 17: equipment cannot be changed now.</summary>
    public const byte NoEquipChange = 17;

    /// <summary>Result code 31: sent when the players are not on the same stream.</summary>
    public const byte NotSameStream = 31;

    /// <summary>Divorce result 2: divorced.</summary>
    public const byte Divorced = 2;

    /// <summary>Divorce result 4: the spouse is away.</summary>
    public const byte SpouseAway = 4;

    /// <summary>Divorce result 5: the divorce was cancelled.</summary>
    public const byte DivorceCancelled = 5;

    /// <summary>Divorce result 7: not married.</summary>
    public const byte NotMarried = 7;

    /// <summary>Divorce result 8: the spouse divorced.</summary>
    public const byte SpouseDivorced = 8;

    /// <summary>Divorce result 9: clears the spouse.</summary>
    public const byte ClearSpouse = 9;

    /// <summary>Tells a player about the application of the partner.</summary>
    /// <param name="partnerId">Id of the partner.</param>
    /// <returns>The packet.</returns>
    public static byte[] Application(int partnerId) => new PacketWriter(Action45.Code, Action45.Application).I32(partnerId).ToArray();

    /// <summary>Sends a marriage result code.</summary>
    /// <param name="code">The result code.</param>
    /// <returns>The packet.</returns>
    public static byte[] Result(byte code) => new PacketWriter(Action45.Code, Action45.Result).U8(code).ToArray();

    /// <summary>Sends a marriage result code with a player id.</summary>
    /// <param name="code">The result code.</param>
    /// <param name="playerId">Id of the player concerned.</param>
    /// <returns>The packet.</returns>
    public static byte[] Result(byte code, int playerId) => new PacketWriter(Action45.Code, Action45.Result).U8(code).I32(playerId).ToArray();

    /// <summary>Sends the fee paid result with the gold left.</summary>
    /// <param name="code">The result code.</param>
    /// <param name="goldLeft">Gold the player has left.</param>
    /// <returns>The packet.</returns>
    public static byte[] FeePaidResult(byte code, int goldLeft) => Result(code, goldLeft);

    /// <summary>Sends the accepted application with its deadline.</summary>
    /// <param name="deadline">The deadline text.</param>
    /// <returns>The packet.</returns>
    public static byte[] AppliedResult(string deadline) =>
        new PacketWriter(Action45.Code, Action45.Result).U8(Applied).PStr8(deadline).ToArray();

    /// <summary>Starts or stops the wedding music.</summary>
    /// <param name="march">True for the march, false otherwise.</param>
    /// <returns>The packet.</returns>
    public static byte[] WeddingMusic(bool march) =>
        new PacketWriter(Action45.Code, Action45.Result).U8(Music).U8(march ? (byte)1 : (byte)0).ToArray();

    /// <summary>Sends a step of the wedding.</summary>
    /// <param name="step">The step number.</param>
    /// <param name="groomId">Id of the groom.</param>
    /// <param name="brideId">Id of the bride.</param>
    /// <returns>The packet.</returns>
    public static byte[] WeddingStep(byte step, int groomId, int brideId) =>
        new PacketWriter(Action45.Code, Action45.WeddingStep).U8(step).I32(groomId).I32(brideId).ToArray();

    /// <summary>Locks or unlocks the player's input.</summary>
    /// <param name="locked">True to lock.</param>
    /// <returns>The packet.</returns>
    public static byte[] InputLock(bool locked) =>
        new PacketWriter(Action45.Code, Action45.InputLock).U8(locked ? (byte)1 : (byte)0).ToArray();

    /// <summary>Announces the start of the ceremony.</summary>
    /// <param name="groomId">Id of the groom.</param>
    /// <param name="brideId">Id of the bride.</param>
    /// <param name="groom">Name of the groom.</param>
    /// <param name="bride">Name of the bride.</param>
    /// <param name="place">Name of the place.</param>
    /// <returns>The packet.</returns>
    public static byte[] CeremonyStarted(
        int groomId,
        int brideId,
        string groom,
        string bride,
        string place) =>
            new PacketWriter(Action45.Code, Action45.Result).U8(CeremonyNames).I32(groomId).I32(brideId).PStr8(groom).PStr8(bride).RawString(place).ToArray();

    /// <summary>Hides or shows a player's pets.</summary>
    /// <param name="playerId">Id of the player.</param>
    /// <param name="hidden">True to hide.</param>
    /// <returns>The packet.</returns>
    public static byte[] PetsHidden(int playerId, bool hidden) =>
        new PacketWriter(Action45.Code, hidden ? Action45.PetHide : Action45.PetShow).I32(playerId).ToArray();

    /// <summary>Sends the id of the player's spouse.</summary>
    /// <param name="spouseId">Id of the spouse.</param>
    /// <returns>The packet.</returns>
    public static byte[] SpouseId(int spouseId) => new PacketWriter(Action45.Code, Action45.SpouseId).I32(spouseId).ToArray();

    /// <summary>Sends the online state of the spouse.</summary>
    /// <param name="online">True when online.</param>
    /// <returns>The packet.</returns>
    public static byte[] SpouseOnline(bool online) =>
        new PacketWriter(Action45.Code, Action45.SpouseOnline).U8(online ? (byte)0 : (byte)3).ToArray();

    /// <summary>Sends the spouse card.</summary>
    /// <param name="s">The data shown.</param>
    /// <param name="client">The client generation the record is written for.</param>
    /// <returns>The packet.</returns>
    public static byte[] SpouseInfo(SpouseCardInfo s, ClientGeneration client)
    {
        var w = new PacketWriter(Action45.Code, Action45.SpouseInfo).I32(s.Id).U8(s.BodyType).U8(0).U8(s.Head).U8(s.Sprite).U8(s.Element);
        for (var i = 0; i < 6; i++)
            w.U16(i < s.Equipment.Length ? s.Equipment[i] : (ushort)0);
        w.I32(s.Colour1).I32(s.Colour2).Hp(s.Hp, client).U16(s.Sp)
            .U16(s.Strength).U16(s.Constitution).U16(s.Intelligence).U16(s.Wisdom).U16(s.Agility)
            .U8(s.Level).I32(s.Exp).U8(s.Reborn).U8(s.Job).U16(s.MapId);
        foreach (var text in new[] { s.Name, s.GuildName, s.Nickname })
        {
            var bytes = GameEncoding.GetBytes(text);
            w.I32(bytes.Length).Bytes(bytes);
        }
        return w.PStr8(s.Place).ToArray();
    }

    /// <summary>Opens the divorce form.</summary>
    /// <param name="otherId">Id of the other player.</param>
    /// <param name="spouseApplied">True when the spouse applied.</param>
    /// <returns>The packet.</returns>
    public static byte[] DivorceForm(int otherId, bool spouseApplied) =>
        new PacketWriter(Action45.Code, Action45.DivorceForm).I32(otherId).U8(spouseApplied ? (byte)1 : (byte)0).ToArray();

    /// <summary>Sends a divorce result code.</summary>
    /// <param name="code">The result code.</param>
    /// <returns>The packet.</returns>
    public static byte[] DivorceResult(byte code) => new PacketWriter(Action45.Code, Action45.DivorceResult).U8(code).ToArray();
}
