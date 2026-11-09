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
    ILogger<FurMailHandlers> logger)
{
    /// <summary>The item id of the Mailbox.</summary>
    public const ushort Mailbox = 39084; // Mailbox

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
        foreach (var letter in c.FurMails)
            session.Send(FurMailMessages.Letter(letter.SenderId, letter.SenderName, letter.Time, letter.Text));
        c.FurMails.Clear();
    }

    /// <summary>Handles <c>14/6</c>: sends a letter to the named character, either to their session or into their box.</summary>
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
        var targetId = target.Account.LoginId;
        var time = GameClock.Now.ToOADate();
        if (world.SessionOf(targetId) is { } online && online.ActiveCharacter is { } onlineCharacter)
        {
            if (BlocklistHandlers.Blocks(onlineCharacter, myId))
            {
                BlocklistHandlers.Refuse(session, 2); // action
                return;
            }
            online.Send(FurMailMessages.Letter(myId, c.Name, time, text));
        }
        else
        {
            if (BlocklistHandlers.Blocks(target.Character, myId))
            {
                BlocklistHandlers.Refuse(session, 2); // action
                return;
            }
            var capacity = options.Value.HouseLife.FurMailCapacity;
            if (capacity > 0 && target.Character.FurMails.Count >= capacity)
            {
                session.Send(new PacketWriter(Action14.Code, Action14.MailboxFull).ToArray());
                return;
            }
            target.Character.FurMails.Add(new FurMail { SenderId = myId, SenderName = c.Name, Time = time, Text = text });
            await store.SaveAsync(target.Account).ConfigureAwait(false);
        }
        session.Send(FurMailMessages.Result(3)); // result
        logger.LogInformation("[{Session}] {Name} leaves a letter in {Target}'s mailbox: {Text}", session.Id, c.Name, target.Character.Name, text);
    }
}
