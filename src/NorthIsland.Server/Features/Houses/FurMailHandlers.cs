using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NorthIsland.Protocol.IO;
using NorthIsland.Protocol.Messages.Player;
using NorthIsland.Protocol.Opcodes;
using NorthIsland.Protocol.Text;

namespace NorthIsland.Server.Features.Houses;

/// <summary>The Mailbox piece: a player who has one placed can send letters to any character, who finds them when they log in.</summary>
/// <param name="store">The account store, to find characters that are offline.</param>
/// <param name="world">The world state, to find online players.</param>
/// <param name="options">The house options, for the mailbox size.</param>
/// <param name="logger">The logger.</param>
public sealed class FurMailHandlers(
    IAccountStore store,
    WorldState world,
    IOptions<HouseOptions> options,
    ILogger<FurMailHandlers> logger) : IDisposable
{
    /// <summary>The item id of the Mailbox.</summary>
    public const ushort Mailbox = 39084; // Mailbox

    /// <summary>Longest letter text in characters, as the client's letter form allows (views/mail/FurMailWindow.xml).</summary>
    public const int MaxText = 152;

    /// <summary>The most letters a box keeps for a character when <see cref="HouseLifeOptions.FurMailCapacity"/> is 0 or less.</summary>
    public const int DefaultCapacity = 100;

    /// <summary>
    /// Held while a letter goes into the box of a character who is offline and their account is saved. Their senders hold no gate of theirs, so
    /// this keeps two of them from filling the box at once, or one from adding a letter while the save of another writes the box.
    /// </summary>
    private readonly SemaphoreSlim _offlineBoxes = new(1, 1);

    /// <summary>Lets go of <see cref="_offlineBoxes"/>; called by the service container when the server stops.</summary>
    public void Dispose() => _offlineBoxes.Dispose();

    /// <summary>Whether a character has a Mailbox placed.</summary>
    /// <param name="c">The character.</param>
    /// <returns>True when a Mailbox piece is among their furniture.</returns>
    public static bool HasMailbox(Character c) => c.Furniture.Exists(f => f.ItemId == Mailbox);

    /// <summary>Tells the client whether the player has a Mailbox.</summary>
    /// <param name="session">The player's session.</param>
    /// <param name="c">The player's character.</param>
    public static void SendState(GameSession session, Character c) =>
        session.Send(FurMailMessages.Result(HasMailbox(c) ? (byte)3 : (byte)4)); // result

    /// <summary>At login sends the Mailbox state and the letters that arrived while the player was offline, then empties the box.</summary>
    /// <param name="session">The player's session.</param>
    /// <param name="c">The player's character.</param>
    public static void SendOnLogin(GameSession session, Character c)
    {
        if (HasMailbox(c))
            SendState(session, c);
        FurMail[] letters;
        lock (c.FurMails) // a sender who saw the character offline may still be adding a letter
        {
            letters = [.. c.FurMails];
            c.FurMails.Clear();
        }
        foreach (var letter in letters)
            session.Send(FurMailMessages.Letter(letter.SenderId, letter.SenderName, letter.Time, letter.Text));
    }

    /// <summary>
    /// Handles <c>14/6</c>: sends a letter to the named character, either to their session when it is the character being played or into their
    /// box. The text is capped at the client's length, the session must have waited the friend mail's wait, and a full box refuses it.
    /// </summary>
    /// <param name="session">The sender's session.</param>
    /// <param name="body">The sender id (four bytes), the length of the name, the name, then the letter text.</param>
    internal async ValueTask OnLetter(GameSession session, ReadOnlyMemory<byte> body)
    {
        if (session.State != SessionState.InWorld || session.ActiveCharacter is not { } c || session.Account is null || body.Length < 5)
            return;
        var nameLength = body.Span[4];
        if (body.Length < 5 + nameLength)
            return;
        var name = GameEncoding.GetString(body.Span.Slice(5, nameLength)).Trim();
        var text = GameEncoding.GetString(body.Span[(5 + nameLength)..]);
        var myId = session.Account.LoginId;
        if (!HasMailbox(c))
        {
            session.Send(FurMailMessages.Result(2)); // result
            return;
        }
        var target = store.All().SelectMany(a => a.Characters.Select(ch => (Account: a, Character: ch)))
            .FirstOrDefault(x => string.Equals(x.Character.Name, GmTag.Strip(name), StringComparison.OrdinalIgnoreCase) && x.Account.LoginId != myId);
        if (target.Character is null || text.Length == 0)
        {
            session.Send(FurMailMessages.Result(1));
            return;
        }
        if (text.Length > MaxText || !FriendHandlers.TakeMailTurn(session))
        {
            logger.LogWarning("[{Session}] {Name}: letter to {Target} refused ({Length} characters, or too soon)", session.Id, c.Name, name,
                text.Length);
            session.Send(FurMailMessages.Result(1));
            return;
        }
        var targetId = target.Account.LoginId;
        var time = GameClock.Now.ToOADate();
        var online = world.SessionOf(targetId);
        var letter = new FurMail { SenderId = myId, SenderName = c.Name, Time = time, Text = text };
        if (online is not null && ReferenceEquals(online.ActiveCharacter, target.Character))
        {
            if (BlocklistHandlers.Blocks(target.Character, myId))
            {
                BlocklistHandlers.Refuse(session, 2); // action
                return;
            }
            online.Send(FurMailMessages.Letter(myId, c.Name, time, text));
        }
        else if (online is not null) // online with another character: its session saves the account
        {
            if (!online.TryExclusiveFrom(session, out var held))
            {
                logger.LogWarning("[{Session}] {Name}: letter to {Target} refused, the target's account is busy or its connection ended", session.Id,
                    c.Name, target.Character.Name);
                session.Send(FurMailMessages.Result(1));
                return;
            }
            using (held)
            {
                if (!TryPost(session, target.Character, letter))
                    return;
            }
        }
        else
        {
            await _offlineBoxes.WaitAsync().ConfigureAwait(false); // no session gate: the semaphore is never held while waiting for one
            try
            {
                if (!TryPost(session, target.Character, letter))
                    return;
                await store.SaveAsync(target.Account).ConfigureAwait(false);
            }
            finally
            {
                _offlineBoxes.Release();
            }
        }
        session.Send(FurMailMessages.Result(3)); // result
        logger.LogInformation("[{Session}] {Name} leaves a letter in {Target}'s mailbox: {Text}", session.Id, c.Name, target.Character.Name, text);
    }

    /// <summary>
    /// Puts a letter into a character's box unless they block the sender or the box is full, telling the sender why it is refused; the caller
    /// holds the box (the target's session gate, or <see cref="_offlineBoxes"/>).
    /// </summary>
    /// <param name="session">The sender's session.</param>
    /// <param name="target">The character the letter is for.</param>
    /// <param name="letter">The letter.</param>
    /// <returns>True when the letter is in the box.</returns>
    private bool TryPost(GameSession session, Character target, FurMail letter)
    {
        if (BlocklistHandlers.Blocks(target, letter.SenderId))
        {
            BlocklistHandlers.Refuse(session, 2); // action
            return false;
        }
        var capacity = options.Value.HouseLife.FurMailCapacity is > 0 and var set ? set : DefaultCapacity;
        lock (target.FurMails) // the character's login may be taking the letters out (SendOnLogin)
        {
            if (target.FurMails.Count >= capacity)
            {
                session.Send(new PacketWriter(Action14.Code, Action14.MailboxFull).ToArray());
                return false;
            }
            target.FurMails.Add(letter);
        }
        return true;
    }
}
