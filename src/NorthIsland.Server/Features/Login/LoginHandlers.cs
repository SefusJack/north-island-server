using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NorthIsland.Protocol;
using NorthIsland.Protocol.IO;
using NorthIsland.Protocol.Messages.Login;
using NorthIsland.Protocol.Opcodes;

namespace NorthIsland.Server.Features.Login;

/// <summary>
/// Handles the login and character-select packets: account login, character list, slot choice, character creation and deletion, and the secret code
/// change.
/// </summary>
/// <param name="store">Account and character storage.</param>
/// <param name="world">The world state, used to find and kick an earlier session of the same account.</param>
/// <param name="itemHandlers">Gives a new character its starter kit.</param>
/// <param name="formulas">Sets the initial values of a new character.</param>
/// <param name="worldEntry">Sends the world-entry packets once a character is chosen.</param>
/// <param name="bans">The account and address ban list checked at login.</param>
/// <param name="options">
/// Server options: server id, accepted client versions, spawn point, auto-registration and what follows character creation.
/// </param>
/// <param name="logger">Logger.</param>
public sealed partial class LoginHandlers(
    IAccountStore store,
    WorldState world,
    ItemHandlers itemHandlers,
    FormulaData formulas,
    WorldEntry worldEntry,
    BanList bans,
    IOptions<ServerOptions> options,
    ILogger<LoginHandlers> logger)
{
    /// <summary>The server options, read once at construction.</summary>
    private readonly ServerOptions _options = options.Value;

    /// <summary>Matches a valid account name: 4 to 14 letters or digits.</summary>
    /// <returns>The compiled expression.</returns>
    [GeneratedRegex("^[A-Za-z0-9]{4,14}$")]
    private static partial Regex AccountNamePattern();

    /// <summary>Matches a valid character name: 1 to 10 letters or digits.</summary>
    /// <returns>The compiled expression.</returns>
    [GeneratedRegex("^[A-Za-z0-9]{1,10}$")]
    private static partial Regex CharacterNamePattern();

    /// <summary>
    /// Changes the active character's secret code and answers with the result: 1 changed, 2 wrong old code, 4 new code shorter than 6 characters.
    /// </summary>
    /// <param name="session">The player's session.</param>
    /// <param name="body">The packet body after the action and sub code.</param>
    internal async ValueTask OnChangePassword(GameSession session, ReadOnlyMemory<byte> body)
    {
        if (session.Account is not { } account || session.ActiveCharacter is not { } character || body.Length < 2)
            return;
        var r = new PacketReader(body.Span);
        var old = r.PStr8();
        var fresh = r.Remaining > 0 ? r.PStr8() : "";
        byte result = !string.IsNullOrEmpty(character.Cipher) && character.Cipher != old ? (byte)2 : fresh.Length < 6 ? (byte)4 : (byte)1;
        if (result == 1)
        {
            character.Cipher = fresh;
            await store.SaveAsync(account).ConfigureAwait(false);
        }
        session.Send(new PacketWriter(Action35.Code, Action35.ChangePassword).U8(result).ToArray());
        logger.LogInformation("[{Session}] {Name}: change secret code -> {Result}", session.Id, character.Name, result);
    }

    /// <summary>
    /// Answers the client's first packet with the server identity and moves the session to the login stage; ignored when the session is not newly
    /// connected.
    /// </summary>
    /// <param name="session">The player's session.</param>
    /// <param name="body">The packet body after the action and sub code.</param>
    internal ValueTask OnHello(GameSession session, ReadOnlyMemory<byte> body)
    {
        if (session.State != SessionState.Connected)
        {
            logger.LogInformation("[{Session}] action 0 in state {State} (kick acknowledge)", session.Id, session.State);
            return ValueTask.CompletedTask;
        }

        session.Send(ServerMessages.ServerSelected(_options.ServerId, _options.ServerFlag, _options.ServerName));
        session.State = SessionState.AwaitLogin;
        return ValueTask.CompletedTask;
    }

    /// <summary>Handles a login by account name and password.</summary>
    /// <param name="session">The player's session.</param>
    /// <param name="body">The packet body after the action and sub code.</param>
    internal async ValueTask OnAccountLogin(GameSession session, ReadOnlyMemory<byte> body)
    {
        var req = AccountLoginRequest.Parse(body.Span);
        await LoginAsync(session, req.Account, req.Password, req.Version, req.ItemDatSize).ConfigureAwait(false);
    }

    /// <summary>Handles a login by a prefix and a number; the account name is the trimmed prefix followed by the number.</summary>
    /// <param name="session">The player's session.</param>
    /// <param name="body">The packet body after the action and sub code.</param>
    internal async ValueTask OnNumericLogin(GameSession session, ReadOnlyMemory<byte> body)
    {
        var req = NumericLoginRequest.Parse(body.Span);
        var account = req.Prefix.Trim() + req.LoginNumber.ToString(System.Globalization.CultureInfo.InvariantCulture);
        await LoginAsync(session, account, req.Password, req.Version, req.ItemDatSize).ConfigureAwait(false);
    }

    /// <summary>
    /// Runs a login: checks the client version, the Item.Dat size, the name format and the bans, verifies the password (creating the account when
    /// auto-registration is on), kicks an earlier session of the account, then sends the login id and the character list.
    /// </summary>
    /// <param name="session">The player's session.</param>
    /// <param name="name">The account name.</param>
    /// <param name="password">The password as typed.</param>
    /// <param name="version">The client version number.</param>
    /// <param name="itemDatSize">Size of the client's Item.Dat, or null when the client did not send one.</param>
    private async Task LoginAsync(
        GameSession session,
        string name,
        string password,
        ushort version,
        long? itemDatSize)
    {
        if (session.State != SessionState.AwaitLogin)
        {
            logger.LogWarning("[{Session}] login in state {State}, ignored", session.Id, session.State);
            return;
        }

        logger.LogInformation("[{Session}] login '{Name}' version {Version} Item.Dat size {ItemDat}",
            session.Id, name, version, itemDatSize?.ToString(System.Globalization.CultureInfo.InvariantCulture) ?? "(none)");

        if (!_options.AcceptsClientVersion(version))
        {
            session.Send(ServerMessages.Kick(KickReason.FalseEdition));
            return;
        }
        session.Client = ClientVersions.For(version);
        if (_options.EnforceItemDatSize && _options.ItemDatSize is { } expected && itemDatSize != expected)
        {
            session.Send(ServerMessages.Kick(KickReason.InvalidItemVersion));
            return;
        }
        if (!AccountNamePattern().IsMatch(name) || password.Length == 0)
        {
            session.Send(ServerMessages.LoginRejected(Action1.WrongAccountOrPassword));
            return;
        }
        if (bans.IsAccountBanned(name) || bans.IsAddressBanned(session.RemoteEndPoint?.Address))
        {
            logger.LogWarning("[{Session}] login '{Name}' from {Address} refused: banned", session.Id, name, session.RemoteEndPoint?.Address);
            session.Send(ServerMessages.Locked(account: true, GmCommands.BanSeconds));
            return;
        }

        var account = await store.FindByNameAsync(name).ConfigureAwait(false);
        if (account is null)
        {
            if (!_options.AutoRegister)
            {
                session.Send(ServerMessages.LoginRejected(Action1.WrongAccountOrPassword));
                return;
            }
            account = await store.CreateAsync(name, password).ConfigureAwait(false);
        }
        else if (!PasswordHasher.Verify(password, account.PasswordHash))
        {
            session.Send(ServerMessages.LoginRejected(Action1.WrongAccountOrPassword));
            return;
        }

        if (world.SessionOf(account.LoginId) is { } previous)
        {
            logger.LogInformation("[{Session}] {Name} logged in again, kicking session {Previous}", session.Id, name, previous.Id);
            previous.Send(ServerMessages.Kick(KickReason.RepeatedLogin));
            previous.Close();
            world.Leave(previous);
        }

        account.LastLoginUtc = GameClock.UtcNow;
        await store.SaveAsync(account).ConfigureAwait(false);
        session.Account = account;

        session.Send(ServerMessages.LoginId(account.LoginId));
        SendCharacterList(session);
    }

    /// <summary>Sends the account's characters ordered by slot and moves the session to character select.</summary>
    /// <param name="session">The player's session.</param>
    private static void SendCharacterList(GameSession session)
    {
        var account = session.Account!;
        session.Send(ServerMessages.CharacterList(account.Characters.OrderBy(c => c.Slot).Select(ToSummary)));
        session.State = SessionState.CharacterSelect;
    }

    /// <summary>
    /// Handles the choice of a slot: enters the world with the character in it, or starts character creation when the slot is empty.
    /// </summary>
    /// <param name="session">The player's session.</param>
    /// <param name="body">The packet body after the action and sub code.</param>
    internal async ValueTask OnSelectSlot(GameSession session, ReadOnlyMemory<byte> body)
    {
        if (session.State != SessionState.CharacterSelect || session.Account is null)
        {
            logger.LogWarning("[{Session}] 63/2 in state {State}, ignored", session.Id, session.State);
            return;
        }

        var slot = SelectSlotRequest.Parse(body.Span).Slot;
        session.Login.SelectedSlot = slot;
        var character = session.Account.CharacterInSlot(slot);
        if (character is null)
        {
            session.Login.PendingName = null;
            session.Send(ServerMessages.StartCharacterCreation(_options.SkipCreationCipher));
            session.State = SessionState.Creating;
            return;
        }

        await worldEntry.EnterWorldAsync(session, character).ConfigureAwait(false);
    }

    /// <summary>Forgets the account and returns the session to the stage that waits for a login.</summary>
    /// <param name="session">The player's session.</param>
    /// <param name="body">The packet body after the action and sub code.</param>
    internal static ValueTask OnBackToLogin(GameSession session, ReadOnlyMemory<byte> body)
    {
        session.Account = null;
        session.State = SessionState.AwaitLogin;
        return ValueTask.CompletedTask;
    }

    /// <summary>Drops the pending character name and returns the session to character select.</summary>
    /// <param name="session">The player's session.</param>
    /// <param name="body">The packet body after the action and sub code.</param>
    internal static ValueTask OnCancelCreation(GameSession session, ReadOnlyMemory<byte> body)
    {
        session.Login.PendingName = null;
        session.State = SessionState.CharacterSelect;
        return ValueTask.CompletedTask;
    }

    /// <summary>
    /// Checks a proposed character name (valid format and not taken) and answers with the result; remembers the name as pending when it is accepted.
    /// </summary>
    /// <param name="session">The player's session.</param>
    /// <param name="body">The packet body after the action and sub code.</param>
    internal async ValueTask OnCheckName(GameSession session, ReadOnlyMemory<byte> body)
    {
        if (session.State != SessionState.Creating)
        {
            logger.LogWarning("[{Session}] 9/2 in state {State}, ignored", session.Id, session.State);
            return;
        }

        var name = CheckNameRequest.Parse(body.Span).Name;
        NameCheck result;
        if (!CharacterNamePattern().IsMatch(name))
            result = NameCheck.Invalid;
        else if (await store.IsCharacterNameTakenAsync(name).ConfigureAwait(false))
            result = NameCheck.Unavailable;
        else
            result = NameCheck.Ok;

        session.Login.PendingName = result == NameCheck.Ok ? name : null;
        session.Send(ServerMessages.NameCheckResult(result));
    }

    /// <summary>
    /// Creates the character in the selected slot from the pending name and the creation choices, then enters the world or resends the character
    /// list, as configured; answers that the name is unavailable when the slot or the name was taken meanwhile.
    /// </summary>
    /// <param name="session">The player's session.</param>
    /// <param name="body">The packet body after the action and sub code.</param>
    internal async ValueTask OnCreateCharacter(GameSession session, ReadOnlyMemory<byte> body)
    {
        if (session.State != SessionState.Creating || session.Account is null || session.Login.PendingName is null)
        {
            logger.LogWarning("[{Session}] 9/1 in state {State} without an accepted name, ignored", session.Id, session.State);
            return;
        }

        var req = CreateCharacterRequest.Parse(body.Span);
        var account = session.Account;
        if (account.CharacterInSlot(session.Login.SelectedSlot) is not null ||
            await store.IsCharacterNameTakenAsync(session.Login.PendingName!).ConfigureAwait(false))
        {
            session.Send(ServerMessages.NameCheckResult(NameCheck.Unavailable));
            return;
        }

        var character = new Character
        {
            Slot = session.Login.SelectedSlot,
            Name = session.Login.PendingName!,
            CreatedUtc = GameClock.UtcNow,
            BodyType = req.BodyType,
            Head = req.Head,
            Sprite = req.Sprite,
            Colour1 = req.Colour1,
            Colour2 = req.Colour2,
            Element = req.Element,
            CreationStats = [.. req.Stats],
            Cipher = req.Cipher,
            MapId = _options.Spawn.MapId,
            X = _options.Spawn.X,
            Y = _options.Spawn.Y,
        };
        worldEntry.ApplyStartingAttributes(character);
        formulas.InitialiseNew(character);
        itemHandlers.EnsureStarterKit(character);
        account.Characters.Add(character);
        await store.SaveAsync(account).ConfigureAwait(false);
        session.Login.PendingName = null;
        logger.LogInformation("[{Session}] created character '{Name}' in slot {Slot} for {Account}",
            session.Id, character.Name, character.Slot, account.Name);

        if (_options.AfterCreate == AfterCreate.EnterWorld)
            await worldEntry.EnterWorldAsync(session, character).ConfigureAwait(false);
        else
            SendCharacterList(session);
    }

    /// <summary>
    /// Deletes the character in the requested slot and answers with a result: 1 deleted, 3 wrong secret code, 4 not possible now or no such
    /// character.
    /// </summary>
    /// <param name="session">The player's session.</param>
    /// <param name="body">The packet body after the action and sub code.</param>
    internal async ValueTask OnDeleteCharacter(GameSession session, ReadOnlyMemory<byte> body)
    {
        var req = DeleteCharacterRequest.Parse(body.Span);
        var account = session.Account;
        var character = account?.CharacterInSlot(req.Slot);
        byte result;
        if (session.State != SessionState.CharacterSelect || account is null || character is null)
            result = 4;
        else if (!string.IsNullOrEmpty(character.Cipher) && character.Cipher != req.Cipher)
            result = 3;
        else
        {
            account.Characters.Remove(character);
            await store.SaveAsync(account).ConfigureAwait(false);
            result = 1;
        }
        session.Send(ServerMessages.DeleteCharacterResult(result, req.Slot));
    }

    /// <summary>Builds the character list entry of a character.</summary>
    /// <param name="c">The character.</param>
    /// <returns>The list entry.</returns>
    private static CharacterSummary ToSummary(Character c) => new()
    {
        Slot = c.Slot,
        Name = c.Name,
        Level = c.Level,
        Element = c.Element,
        MaxHp = c.MaxHp,
        Hp = c.Hp,
        MaxSp = c.MaxSp,
        Sp = c.Sp,
        Exp = c.Exp,
        Gold = c.Gold,
        BodyType = c.BodyType,
        Head = c.Head,
        Sprite = c.Sprite,
        Colour1 = c.Colour1,
        Colour2 = c.Colour2,
        RebornCount = c.RebornCount,
        Job = c.Job,
        EquippedItems = [.. c.EquippedItemIds()],
    };
}
