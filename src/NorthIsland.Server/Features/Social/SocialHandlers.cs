using Microsoft.Extensions.Logging;
using NorthIsland.Protocol.IO;
using NorthIsland.Protocol.Messages.World;
using NorthIsland.Protocol.Opcodes;

namespace NorthIsland.Server.Features.Social;

/// <summary>Handles small player settings and actions: options, actions, nickname, profile, hotkeys, emotions.</summary>
/// <param name="world">World state, for broadcasts.</param>
/// <param name="options">Item options, with the Yes-No Board emotions.</param>
/// <param name="logger">Logger.</param>
public sealed class SocialHandlers(
    WorldState world,
    Microsoft.Extensions.Options.IOptions<ItemOptions> options,
    ILogger<SocialHandlers> logger)
{
    /// <summary>Stores the packet's byte as the character's first option.</summary>
    /// <param name="s">The player's session.</param>
    /// <param name="b">The packet body.</param>
    internal ValueTask OnOption1(GameSession s, ReadOnlyMemory<byte> b) => OnOption(s, b, c => c.Option1 = b.Span[0]);

    /// <summary>Stores the packet's byte as the character's second option.</summary>
    /// <param name="s">The player's session.</param>
    /// <param name="b">The packet body.</param>
    internal ValueTask OnOption2(GameSession s, ReadOnlyMemory<byte> b) => OnOption(s, b, c => c.Option2 = b.Span[0]);

    /// <summary>Stores the packet's byte as the character's sixth option.</summary>
    /// <param name="s">The player's session.</param>
    /// <param name="b">The packet body.</param>
    internal ValueTask OnOption6(GameSession s, ReadOnlyMemory<byte> b) => OnOption(s, b, c => c.Option6 = b.Span[0]);

    /// <summary>Stores the packet's byte as the character's party option.</summary>
    /// <param name="s">The player's session.</param>
    /// <param name="b">The packet body.</param>
    internal ValueTask OnPartyOption(GameSession s, ReadOnlyMemory<byte> b) => OnOption(s, b, c => c.PartyOption = b.Span[0]);

    /// <summary>Stores the packet's byte as the character's chat channel bits.</summary>
    /// <param name="s">The player's session.</param>
    /// <param name="b">The packet body.</param>
    internal ValueTask OnChannelBits(GameSession s, ReadOnlyMemory<byte> b) => OnOption(s, b, c => c.ChannelBits = b.Span[0]);

    /// <summary>Shows an action frame of the player to the others on the map.</summary>
    /// <param name="session">The player's session.</param>
    /// <param name="body">The packet body: the frame byte.</param>
    internal ValueTask OnAction(GameSession session, ReadOnlyMemory<byte> body)
    {
        if (InWorld(session) is null)
            return ValueTask.CompletedTask;
        var frame = new PacketReader(body.Span).U8();
        world.Broadcast(session, WorldServerMessages.PlayerAction(session.Account!.LoginId, frame), sameMapOnly: true);
        return ValueTask.CompletedTask;
    }

    /// <summary>Stores the nickname (cut to 14 characters) and tells every player.</summary>
    /// <param name="session">The player's session.</param>
    /// <param name="body">The packet body: a length-prefixed nickname.</param>
    internal ValueTask OnNickname(GameSession session, ReadOnlyMemory<byte> body)
    {
        if (InWorld(session) is not { } character)
            return ValueTask.CompletedTask;
        var nickname = new PacketReader(body.Span).PStr8();
        if (nickname.Length > 14)
            nickname = nickname[..14];
        character.Nickname = nickname;
        logger.LogInformation("[{Session}] {Name} nickname '{Nickname}'", session.Id, character.Name, nickname);
        world.Broadcast(session, WorldServerMessages.Nickname(session.Account!.LoginId, nickname), sameMapOnly: false);
        return ValueTask.CompletedTask;
    }

    /// <summary>Stores the profile (heart number, blood type, birthday) and shows the heart number on the map.</summary>
    /// <param name="session">The player's session.</param>
    /// <param name="body">The packet body: the profile fields.</param>
    internal ValueTask OnProfile(GameSession session, ReadOnlyMemory<byte> body)
    {
        if (InWorld(session) is not { } character)
            return ValueTask.CompletedTask;
        var p = ProfileUpdate.Parse(body.Span);
        character.HeartNumber = p.HeartNumber;
        character.BloodType = p.BloodType;
        character.BirthYear = p.BirthYear;
        character.BirthMonth = p.BirthMonth;
        character.BirthDay = p.BirthDay;
        logger.LogInformation("[{Session}] {Name} profile heart {Heart}, byte {Byte}, birthday {Y}-{M}-{D}",
            session.Id, character.Name, p.HeartNumber, p.BloodType, p.BirthYear, p.BirthMonth, p.BirthDay);
        world.Broadcast(session, WorldServerMessages.HeartNumber(session.Account!.LoginId, p.HeartNumber), sameMapOnly: true);
        return ValueTask.CompletedTask;
    }

    /// <summary>Stores one option byte through a setter and logs the options.</summary>
    /// <param name="session">The player's session.</param>
    /// <param name="body">The packet body; at least one byte.</param>
    /// <param name="apply">Writes the value into the character.</param>
    private ValueTask OnOption(GameSession session, ReadOnlyMemory<byte> body, Action<Character> apply)
    {
        if (InWorld(session) is not { } character || body.Length < 1)
            return ValueTask.CompletedTask;
        apply(character);
        logger.LogInformation("[{Session}] {Name} options {O1}/{O2}/{O6} party {Party} channels {Bits:X2}",
            session.Id, character.Name, character.Option1, character.Option2, character.Option6, character.PartyOption, character.ChannelBits);
        return ValueTask.CompletedTask;
    }

    /// <summary>Stores the two constellations.</summary>
    /// <param name="session">The player's session.</param>
    /// <param name="body">The packet body: two bytes.</param>
    internal ValueTask OnConstellation(GameSession session, ReadOnlyMemory<byte> body)
    {
        if (InWorld(session) is not { } character)
            return ValueTask.CompletedTask;
        var r = new PacketReader(body.Span);
        character.Constellation1 = r.U8();
        character.Constellation2 = r.U8();
        logger.LogInformation("[{Session}] {Name} constellation {A}/{B}", session.Id, character.Name, character.Constellation1, character.Constellation2);
        return ValueTask.CompletedTask;
    }

    /// <summary>Removes a quest mark the player gives up.</summary>
    /// <param name="session">The player's session.</param>
    /// <param name="body">The packet body: the mark id.</param>
    internal ValueTask OnAbandonQuest(GameSession session, ReadOnlyMemory<byte> body)
    {
        if (InWorld(session) is not { } character || body.Length < 2)
            return ValueTask.CompletedTask;
        var id = new PacketReader(body.Span).U16();
        if (character.Marks.RemoveAll(m => m.Id == id) == 0)
            return ValueTask.CompletedTask;
        session.Send(Protocol.Messages.Events.MarkMessages.Delete(id));
        logger.LogInformation("[{Session}] {Name} abandons quest mark {Mark}", session.Id, character.Name, id);
        return ValueTask.CompletedTask;
    }

    /// <summary>Stores a hotkey of the player: kind, id, page (1 to 3) and slot (1 to 8); an empty id clears the slot.</summary>
    /// <param name="session">The player's session.</param>
    /// <param name="body">The packet body: kind, id, page, slot.</param>
    internal static ValueTask OnHotkey(GameSession session, ReadOnlyMemory<byte> body)
    {
        if (InWorld(session) is not { } character || body.Length < 5)
            return ValueTask.CompletedTask;
        var r = new PacketReader(body.Span);
        var (kind, id, page, slot) = (r.U8(), r.U16(), r.U8(), r.U8());
        if (page is < 1 or > 3 || slot is < 1 or > 8)
            return ValueTask.CompletedTask;
        character.Hotkeys.RemoveAll(h => h.Page == page && h.Slot == slot);
        if (kind is 1 or 2 && id != 0)
            character.Hotkeys.Add(new Hotkey { Page = page, Slot = slot, Kind = kind, Id = id });
        return ValueTask.CompletedTask;
    }

    /// <summary>Shows an emotion of the player to the others on the map; ignored in battle.</summary>
    /// <param name="session">The player's session.</param>
    /// <param name="body">The packet body: the emotion byte.</param>
    internal ValueTask OnEmotion(GameSession session, ReadOnlyMemory<byte> body)
    {
        if (InWorld(session) is null || body.Length < 1 || session.Battle is not null)
            return ValueTask.CompletedTask;
        world.Broadcast(session, WorldServerMessages.PlayerEmotion(session.Account!.LoginId, body.Span[0]), sameMapOnly: true); // emotion
        return ValueTask.CompletedTask;
    }

    /// <summary>Handles the Yes-No Board: shows the emotion for yes or no (random for any other choice) to the player and the map.</summary>
    /// <param name="session">The player's session.</param>
    /// <param name="body">The packet body: the bag slot of the board and the choice.</param>
    internal ValueTask OnYesNo(GameSession session, ReadOnlyMemory<byte> body)
    {
        if (InWorld(session) is not { } c || body.Length < 2 || session.Battle is not null
            || c.Bag.Find(b => b.Slot == body.Span[0])?.ItemId != 34378) // 34378 Yes-No Board
            return ValueTask.CompletedTask;
        var choice = body.Span[1] is 1 or 2 ? body.Span[1] : (byte)(GameRandom.Shared.Next(2) + 1);
        var o = options.Value.ItemUse;
        var packet = new PacketWriter(Action32.Code, Action32.ShowEmotion)
            .I32(session.Account!.LoginId)
            .U8(choice == 1 ? o.YesNoEmotionO : o.YesNoEmotionX)
            .ToArray();
        session.Send(packet);
        world.Broadcast(session, packet, sameMapOnly: true);
        return ValueTask.CompletedTask;
    }

    /// <summary>Ends the player's action on the map by showing frame 0.</summary>
    /// <param name="session">The player's session.</param>
    /// <param name="body">The packet body; not read.</param>
    internal ValueTask OnStopAction(GameSession session, ReadOnlyMemory<byte> body)
    {
        if (InWorld(session) is null)
            return ValueTask.CompletedTask;
        world.Broadcast(session, WorldServerMessages.PlayerAction(session.Account!.LoginId, 0), sameMapOnly: true);
        return ValueTask.CompletedTask;
    }

    /// <summary>Accepts a packet and does nothing.</summary>
    /// <param name="session">The player's session.</param>
    /// <param name="body">The packet body; not read.</param>
    internal static ValueTask Accept(GameSession session, ReadOnlyMemory<byte> body) => ValueTask.CompletedTask;

    /// <summary>Gives the active character of a session that is in the world.</summary>
    /// <param name="session">The player's session.</param>
    /// <returns>The character, or null when the session is not in the world.</returns>
    private static Character? InWorld(GameSession session)
    {
        var character = session.ActiveCharacter;
        return session.State == SessionState.InWorld && character is not null && session.Account is not null ? character : null;
    }
}
