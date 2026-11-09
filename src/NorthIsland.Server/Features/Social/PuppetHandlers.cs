using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NorthIsland.Protocol.IO;
using NorthIsland.Protocol.Messages.Items;
using NorthIsland.Protocol.Messages.Player;
using NorthIsland.Protocol.Opcodes;
using NorthIsland.Protocol.Text;

namespace NorthIsland.Server.Features.Social;

/// <summary>Handles the puppet training (Memory Card) between a requester and a helper.</summary>
/// <param name="world">World state.</param>
/// <param name="store">The account store, to find and save offline players.</param>
/// <param name="experience">Experience service, whose Earned event feeds the cards and which pays out card exp.</param>
/// <param name="options">Social options, with the puppet settings.</param>
/// <param name="logger">Logger.</param>
public sealed class PuppetHandlers(
    WorldState world,
    IAccountStore store,
    Experience experience,
    IOptions<SocialOptions> options,
    ILogger<PuppetHandlers> logger)
{
    /// <summary>Item ids of the Memory Card and of the Strong Scroll that expands its capacity.</summary>
    public const ushort MemoryCard = 34232, StrongScroll = 30101; // 34232 Memory Card, 30101 Strong Scroll

    /// <summary>
    /// Card exp capacity added by each step (<c>StepExp</c>), the highest step (<c>MaxStep</c>) and the number of cards a helper can hold
    /// (<c>Cards</c>).
    /// </summary>
    public const int StepExp = 500_000, MaxStep = 10, Cards = 3;

    /// <summary>Open invitations by helper login id: the requester's login id, the bag slot of their card and when it was sent.</summary>
    private readonly Dictionary<int, (int Requester, byte Slot, DateTime At)> _invites = [];

    /// <summary>Guards the invitations and the puppet state of characters.</summary>
    private readonly Lock _lock = new();

    /// <summary>Subscribes to exp earned and to players leaving.</summary>
    internal void Start()
    {
        experience.Earned += OnEarned;
        world.Leaving += OnLeaving;
    }

    /// <summary>Computes the exp a requester's card can hold: one <c>StepExp</c> more for each step.</summary>
    /// <param name="c">The requester's character.</param>
    /// <returns>The capacity in exp.</returns>
    private static int Capacity(Character c) => (c.MemCardStep + 1) * StepExp;

    /// <summary>Builds a puppet packet (<c>23/104</c>) with the option byte and a body.</summary>
    /// <param name="option">The option byte after the action and sub code.</param>
    /// <param name="body">Writes the rest of the packet.</param>
    /// <returns>The packet bytes.</returns>
    private static byte[] P(byte option, Action<PacketWriter> body)
    {
        var w = new PacketWriter(Action23.Code, Action23.Puppet).U8(option);
        body(w);
        return w.ToArray();
    }

    /// <summary>Builds the puppet answer packet (option 3) with a result code.</summary>
    /// <param name="code">The result code.</param>
    /// <returns>The packet bytes.</returns>
    private static byte[] Result(byte code) => P(3, w => w.U8(code));

    /// <summary>Finds a player by login id, online or not.</summary>
    /// <param name="id">The login id.</param>
    /// <returns>
    /// The character (for an offline account the one that holds puppet state, else the first), its session (null when offline) and its account.
    /// </returns>
    private (Character? Character, GameSession? Session, Account? Account) Find(int id)
    {
        if (world.SessionOf(id) is { } session && session.ActiveCharacter is { } active)
            return (active, session, session.Account);
        var account = store.All().FirstOrDefault(a => a.LoginId == id);
        return (account?.Characters.FirstOrDefault(c => c.PuppetHelperId != 0 || c.PuppetCards.Count > 0 || c.MemCardExp > 0)
            ?? account?.Characters.FirstOrDefault(),
            null, account);
    }

    /// <summary>Tells whether a player is in a trade or has a stall open.</summary>
    /// <param name="s">The player's session.</param>
    /// <returns>True when busy.</returns>
    private static bool Busy(GameSession s) => s.Trades.Items is not null || s.Trades.Pets is not null || s.Stalls.Any;

    /// <summary>
    /// Handles a puppet packet; the first body byte selects the step (invite, accept, refuse, cancel, end, use exp, form, mail, time left, grow).
    /// </summary>
    /// <param name="session">The player's session.</param>
    /// <param name="body">The packet body after the action and sub code.</param>
    internal ValueTask OnPacket(GameSession session, ReadOnlyMemory<byte> body)
    {
        if (session.State != SessionState.InWorld || session.ActiveCharacter is not { } c || session.Account is null || body.Length < 1)
            return ValueTask.CompletedTask;
        var id = session.Account.LoginId;
        var span = body.Span;
        lock (_lock)
        {
            Expire(id, c);
            switch (span[0])
            {
                case 1 when span.Length >= 6: // 23/104/1: invite a helper to use the card in a bag slot
                    Invite(session, c, id, System.Buffers.Binary.BinaryPrimitives.ReadInt32LittleEndian(span[1..]), span[5]);
                    break;
                case 2 when span.Length >= 5: // 23/104/2: the helper accepts the invitation
                    Accept(session, c, id, System.Buffers.Binary.BinaryPrimitives.ReadInt32LittleEndian(span[1..]));
                    break;
                case 3 when span.Length >= 6: // 23/104/3: the helper refuses the invitation
                    Refuse(id, span[1], System.Buffers.Binary.BinaryPrimitives.ReadInt32LittleEndian(span[2..]));
                    break;
                case 4 when span.Length >= 2: // 23/104/4: sends a result code to the requesters of the cards held
                    ToHelperOfRequester(c, Result(span[1]));
                    break;
                case 5: // 23/104/5: the requester asks to cancel, passed to the helper
                    AskCancel(c, id);
                    break;
                case 6 when span.Length >= 5: // 23/104/6: ends a relation, sent by the helper
                    EndIf(System.Buffers.Binary.BinaryPrimitives.ReadInt32LittleEndian(span[1..]), id, 10);
                    break;
                case 7: // 23/104/7: pays the card's exp into their experience
                    UseExp(session, c, id);
                    break;
                case 8 when span.Length >= 5: // 23/104/8: the helper asks to cancel, passed to the requester
                    AskCancelByHelper(c, id, System.Buffers.Binary.BinaryPrimitives.ReadInt32LittleEndian(span[1..]));
                    break;
                case 9: // 23/104/9: ends a relation, sent by the requester
                    EndIf(id, c.PuppetHelperId, 10);
                    break;
                case 10 when span.Length >= 6: // 23/104/10: sends a result code to the player named in the packet
                    SendTo(System.Buffers.Binary.BinaryPrimitives.ReadInt32LittleEndian(span[2..]), Result(span[1]));
                    break;
                case 12: // 23/104/12: opens the form, sends the helper and the card exp
                    OpenForm(session, c, id);
                    break;
                case 13 when span.Length >= 6: // 23/104/13: sends a mail to the partner of their relation
                    Mail(c, id, span[5..]);
                    break;
                case 14 when span.Length >= 2: // 23/104/14: asks the minutes left of a relation
                    TimeLeft(session, c, span[1]);
                    break;
                case 15: // 23/104/15: grows the card capacity using Strong Scrolls
                    Grow(session, c, id);
                    break;
            }
        }
        return ValueTask.CompletedTask;
    }

    /// <summary>Sends a packet to an online player; nothing happens when offline.</summary>
    /// <param name="id">The player's login id.</param>
    /// <param name="packet">The packet bytes.</param>
    private void SendTo(int id, byte[] packet)
    {
        if (world.SessionOf(id) is { } s)
            s.Send(packet);
    }

    /// <summary>
    /// Handles a requester's invitation: checks the card and both sides and either sends the invitation to the helper or answers with a refusal code.
    /// </summary>
    /// <param name="session">The requester's session.</param>
    /// <param name="c">The requester's character.</param>
    /// <param name="id">The requester's login id.</param>
    /// <param name="helperId">The invited player's login id.</param>
    /// <param name="bagSlot">The bag slot of the Memory Card.</param>
    private void Invite(
        GameSession session,
        Character c,
        int id,
        int helperId,
        byte bagSlot)
    {
        var card = c.Bag.Find(b => b.Slot == bagSlot);
        byte? refusal =
            card is null || card.ItemId != MemoryCard ? (byte)0
                : Busy(session) ? (byte)(session.Stalls.Any ? 2 : 1)
                    : session.Event is not null || session.Battle is not null ? (byte)4
                        : c.PuppetHelperId != 0 || c.MemCardExp != 0 || c.Level < options.Value.Puppet.MinLevel || helperId == id ? (byte)8
                            : null;
        if (refusal is null)
        {
            var (helper, hs, _) = Find(helperId);
            refusal = hs is null || helper is null ? (byte)9
                : Busy(hs) ? (byte)17
                    : hs.Event is not null ? (byte)5
                        : hs.Battle is not null || _invites.ContainsKey(helperId) && GameClock.UtcNow - _invites[helperId].At < TimeSpan.FromSeconds(8)
                            ? (byte)15
                            : helper.Level < options.Value.Puppet.MinLevel
                                || helper.PuppetHelperId != 0
                                || helper.MemCardExp != 0
                                || helper.PuppetCards.Count >= Cards
                                || helper.PuppetCards.Exists(k => k.RequesterId == id) ? (byte)8
                                : null;
            if (refusal is null)
            {
                _invites[helperId] = (id, bagSlot, GameClock.UtcNow);
                hs!.Send(P(1, w => w.I32(helperId).I32(id)));
                return;
            }
        }
        session.Send(Result(refusal.Value));
    }

    /// <summary>Handles the helper's acceptance: starts the relation, adds a card to the helper and informs both players.</summary>
    /// <param name="session">The helper's session.</param>
    /// <param name="helper">The helper's character.</param>
    /// <param name="helperId">The helper's login id.</param>
    /// <param name="requesterId">The requester's login id.</param>
    private void Accept(GameSession session, Character helper, int helperId, int requesterId)
    {
        if (!_invites.Remove(helperId, out var invite) || invite.Requester != requesterId || GameClock.UtcNow - invite.At > TimeSpan.FromSeconds(30))
            return;
        var (requester, rs, _) = Find(requesterId);
        if (requester is null || rs is null || helper.PuppetCards.Count >= Cards || requester.PuppetHelperId != 0)
        {
            session.Send(Result(9));
            return;
        }
        var slot = Enumerable.Range(1, Cards).First(s => !helper.PuppetCards.Exists(k => k.Slot == s));
        var now = GameClock.UtcNow;
        helper.PuppetCards.Add(new PuppetCard { RequesterId = requesterId, Slot = (byte)slot, Since = now });
        (requester.PuppetHelperId, requester.PuppetBagSlot, requester.PuppetSince) = (helperId, invite.Slot, now);
        rs.Send(P(2, w => w.I32(requesterId).U8(invite.Slot).I32(helperId).I32(helper.Colour1).I32(helper.Colour2)));
        rs.Send(P(12, w => w.I32(requesterId).I32(requester.MemCardExp)));
        session.Send(P(4, w => w.U8(2).U8(0)));
        SendCard(session, helper.PuppetCards.Last());
        logger.LogInformation("{Helper} plays for {Requester} (card slot {Slot})", helper.Name, requester.Name, slot);
    }

    /// <summary>Handles the helper's refusal: drops the invitation and tells the requester.</summary>
    /// <param name="helperId">The helper's login id.</param>
    /// <param name="reason">The refusal code.</param>
    /// <param name="requesterId">The requester's login id.</param>
    private void Refuse(int helperId, byte reason, int requesterId)
    {
        if (_invites.TryGetValue(helperId, out var invite) && invite.Requester == requesterId)
            _invites.Remove(helperId);
        SendTo(requesterId, Result(reason));
    }

    /// <summary>Sends one card entry to the helper's form.</summary>
    /// <param name="helperSession">The helper's session.</param>
    /// <param name="card">The card to describe.</param>
    private void SendCard(GameSession helperSession, PuppetCard card)
    {
        var (requester, rs, _) = Find(card.RequesterId);
        if (requester is null)
            return;
        var guild = world.SessionOf(card.RequesterId) is not null ? "" : "";
        helperSession.Send(
            P(13, w => w.I32(helperSession.Account!.LoginId).U8(card.Slot).I32(requester.MemCardExp).U8(rs is null ? (byte)0 : (byte)1).I32(card.RequesterId)
                .U8(requester.Level)
                .U8(requester.Element)
                .U8(requester.Job)
                .U8(requester.Head)
                .U8(requester.BodyType)
                .I32(requester.Colour1)
                .I32(requester.Colour2)
                .PStr8(requester.Name).PStr8("").PStr8(guild).U8(requester.MemCardStep)));
    }

    /// <summary>Sends the requester the state of their training form: the helper and the card exp.</summary>
    /// <param name="session">The requester's session.</param>
    /// <param name="c">The requester's character.</param>
    /// <param name="id">The requester's login id.</param>
    private void OpenForm(GameSession session, Character c, int id)
    {
        if (c.PuppetHelperId == 0)
            return;
        var (helper, hs, _) = Find(c.PuppetHelperId);
        if (helper is null)
            return;
        session.Send(P(14, w => w.I32(id).PStr8(helper.Name).I32(helper.Colour1).I32(helper.Colour2).I32(c.PuppetHelperId).U8(hs is null ? (byte)0 : (byte)1)
            .U8(helper.Head).U8(helper.BodyType).I32(c.MemCardExp)));
    }

    /// <summary>Copies the share of earned exp onto the card of each requester of a helper; a full card ends the relation.</summary>
    /// <param name="session">The helper's session.</param>
    /// <param name="exp">The exp the helper earned.</param>
    private void OnEarned(GameSession session, int exp)
    {
        if (exp <= 0 || session.ActiveCharacter is not { } helper || session.Account is null || helper.PuppetCards.Count == 0)
            return;
        var id = session.Account.LoginId;
        var share = (int)((long)exp * options.Value.Puppet.SharePercent / 100);
        lock (_lock)
        {
            Expire(id, helper);
            foreach (var card in helper.PuppetCards.ToList())
            {
                var (requester, rs, account) = Find(card.RequesterId);
                if (requester is null)
                    continue;
                var capacity = Capacity(requester);
                requester.MemCardExp = (int)Math.Min(capacity, (long)requester.MemCardExp + share);
                rs?.Send(P(12, w => w.I32(card.RequesterId).I32(requester.MemCardExp)));
                session.Send(P(8, w => w.I32(id).I32(card.RequesterId).I32(requester.MemCardExp).U8(card.Slot)));
                if (account is not null && rs is null)
                    _ = store.SaveAsync(account);
                if (requester.MemCardExp >= capacity)
                {
                    End(card.RequesterId, id, 19);
                    session.Send(Result(16));
                }
            }
        }
    }

    /// <summary>Pays the exp on the requester's card into their experience and empties the card; refused during a relation.</summary>
    /// <param name="session">The requester's session.</param>
    /// <param name="c">The requester's character.</param>
    /// <param name="id">The requester's login id.</param>
    private void UseExp(GameSession session, Character c, int id)
    {
        if (c.MemCardExp <= 0 || c.PuppetHelperId != 0 || c.PuppetCards.Count > 0)
            return;
        var exp = c.MemCardExp;
        c.MemCardExp = 0;
        session.Send(P(12, w => w.I32(id).I32(0)));
        experience.Gain(session, exp);
        logger.LogInformation("{Name} spends {Exp} exp from the Memory Card", c.Name, exp);
    }

    /// <summary>Expands the card capacity by one step by taking Strong Scrolls from the bag (2 for the first step, 3 after).</summary>
    /// <param name="session">The requester's session.</param>
    /// <param name="c">The requester's character.</param>
    /// <param name="id">The requester's login id.</param>
    private static void Grow(GameSession session, Character c, int id)
    {
        if (c.PuppetHelperId != 0 || c.PuppetCards.Count > 0 || c.MemCardStep >= MaxStep)
            return;
        var need = c.MemCardStep == 0 ? 2 : 3;
        var stacks = c.Bag.Where(b => b.ItemId == StrongScroll).ToList();
        if (stacks.Sum(b => b.Count) < need)
            return;
        foreach (var stack in stacks)
        {
            var take = Math.Min(need, stack.Count);
            stack.Count -= (byte)take;
            need -= take;
            if (stack.Count == 0)
                c.Bag.Remove(stack);
            session.Send(ItemMessages.BagSlotDecrease(stack.Slot, (byte)take));
            if (need == 0)
                break;
        }
        c.MemCardStep++;
        session.Send(P(22, w => w.I32(id).U8(c.MemCardStep).U8(1)));
    }

    /// <summary>Sends the minutes left of a relation.</summary>
    /// <param name="session">The player's session.</param>
    /// <param name="c">The player's character.</param>
    /// <param name="slot">The helper's card slot; ignored for a requester.</param>
    private void TimeLeft(GameSession session, Character c, byte slot)
    {
        var since = c.PuppetHelperId != 0 ? c.PuppetSince : c.PuppetCards.Find(k => k.Slot == slot)?.Since;
        if (since is not { } start)
            return;
        var left = TimeSpan.FromDays(options.Value.Puppet.Days) - (GameClock.UtcNow - start);
        session.Send(P(21, w => w.U8(slot).U16((ushort)Math.Clamp((int)left.TotalMinutes, 0, ushort.MaxValue))));
    }

    /// <summary>Passes a requester's cancel request on to their helper.</summary>
    /// <param name="c">The requester's character.</param>
    /// <param name="id">The requester's login id.</param>
    private void AskCancel(Character c, int id)
    {
        if (c.PuppetHelperId != 0)
            SendTo(c.PuppetHelperId, P(5, w => w.I32(id).I32(c.PuppetHelperId)));
    }

    /// <summary>Passes a helper's cancel request on to the requester, if they hold a card for them.</summary>
    /// <param name="helper">The helper's character.</param>
    /// <param name="id">The helper's login id.</param>
    /// <param name="requesterId">The requester's login id.</param>
    private void AskCancelByHelper(Character helper, int id, int requesterId)
    {
        if (helper.PuppetCards.Exists(k => k.RequesterId == requesterId))
            SendTo(requesterId, P(10, w => w.I32(requesterId).I32(id)));
    }

    /// <summary>Sends a packet to the requester of every card the character holds.</summary>
    /// <param name="c">The helper's character.</param>
    /// <param name="packet">The packet bytes.</param>
    private void ToHelperOfRequester(Character c, byte[] packet)
    {
        foreach (var card in c.PuppetCards)
            SendTo(card.RequesterId, packet);
    }

    /// <summary>Ends a relation only when the requester really has that helper.</summary>
    /// <param name="requesterId">The requester's login id.</param>
    /// <param name="helperId">The helper's login id.</param>
    /// <param name="result">The result code sent to both.</param>
    private void EndIf(int requesterId, int helperId, byte result)
    {
        if (Find(requesterId).Character is { } r && r.PuppetHelperId == helperId && helperId != 0)
            End(requesterId, helperId, result);
    }

    /// <summary>Ends a relation: clears the requester, removes the helper's card, tells both players and saves offline accounts.</summary>
    /// <param name="requesterId">The requester's login id.</param>
    /// <param name="helperId">The helper's login id.</param>
    /// <param name="result">The result code sent to both.</param>
    private void End(int requesterId, int helperId, byte result)
    {
        var (requester, rs, racc) = Find(requesterId);
        var (helper, hs, hacc) = Find(helperId);
        if (requester is null || helper is null)
            return;
        var card = helper.PuppetCards.Find(k => k.RequesterId == requesterId);
        var bagSlot = requester.PuppetBagSlot;
        requester.PuppetHelperId = 0;
        requester.PuppetBagSlot = 0;
        if (card is not null)
            helper.PuppetCards.Remove(card);
        rs?.Send(P(6, w => w.I32(requesterId).U8(bagSlot)));
        rs?.Send(P(7, w => w.I32(requesterId)));
        rs?.Send(P(19, w => w.I32(requesterId).U8(bagSlot)));
        rs?.Send(Result(result));
        if (card is not null)
        {
            hs?.Send(P(9, w => w.I32(helperId).U8(card.Slot)));
            if (result != 19)
                hs?.Send(Result(result));
        }
        if (rs is null && racc is not null)
            _ = store.SaveAsync(racc);
        if (hs is null && hacc is not null)
            _ = store.SaveAsync(hacc);
        logger.LogInformation("Puppet training of {Requester} ends", requester.Name);
    }

    /// <summary>Ends the relations of a player that have lasted longer than the allowed days.</summary>
    /// <param name="id">The player's login id.</param>
    /// <param name="c">The player's character.</param>
    private void Expire(int id, Character c)
    {
        var limit = TimeSpan.FromDays(options.Value.Puppet.Days);
        var now = GameClock.UtcNow;
        if (c.PuppetHelperId != 0 && now - c.PuppetSince > limit)
            End(id, c.PuppetHelperId, 14);
        foreach (var card in c.PuppetCards.Where(k => now - k.Since > limit).ToList())
            End(card.RequesterId, id, 14);
    }

    /// <summary>Sends a mail from a player to the partner of their relation whose name matches.</summary>
    /// <param name="c">The sender's character.</param>
    /// <param name="id">The sender's login id.</param>
    /// <param name="rest">The packet bytes: a length-prefixed name, then the text.</param>
    private void Mail(Character c, int id, ReadOnlySpan<byte> rest)
    {
        if (rest.Length < 1 || rest.Length < 1 + rest[0])
            return;
        var name = GameEncoding.GetString(rest.Slice(1, rest[0]));
        var text = GameEncoding.GetString(rest[(1 + rest[0])..]);
        var partners = c.PuppetHelperId != 0 ? [c.PuppetHelperId] : c.PuppetCards.Select(k => k.RequesterId).ToList();
        foreach (var pid in partners)
        {
            var (partner, ps, pacc) = Find(pid);
            if (partner is null || partner.Name != name || text.Length == 0)
                continue;
            var time = GameClock.Now.ToOADate();
            if (ps is not null)
                ps.Send(FriendMessages.Mail(id, time, text));
            else
            {
                partner.Mailbox.Add(new Mail { SenderId = id, Time = time, Text = text });
                if (pacc is not null)
                    _ = store.SaveAsync(pacc);
            }
        }
    }

    /// <summary>Sends the puppet state on login: card step, relation, cards, and notices to the partners who are online.</summary>
    /// <param name="session">The player's session.</param>
    /// <param name="c">The player's character.</param>
    public void SendOnLogin(GameSession session, Character c)
    {
        var id = session.Account!.LoginId;
        lock (_lock)
        {
            Expire(id, c);
            if (c.MemCardStep > 0)
                session.Send(P(22, w => w.I32(id).U8(c.MemCardStep).U8(0)));
            if (c.PuppetHelperId != 0)
            {
                session.Send(P(4, w => w.U8(1).U8(c.PuppetBagSlot)));
                session.Send(P(12, w => w.I32(id).I32(c.MemCardExp)));
                if (world.SessionOf(c.PuppetHelperId) is { } hs && hs.ActiveCharacter is { } h)
                {
                    session.Send(P(18, _ => { }));
                    hs.Send(P(16, w => w.I32(c.PuppetHelperId).I32(id).U8(c.Level)));
                }
            }
            else if (c.MemCardExp > 0)
                session.Send(P(12, w => w.I32(id).I32(c.MemCardExp)));
            if (c.PuppetCards.Count > 0)
            {
                session.Send(P(4, w => w.U8(2).U8(0)));
                foreach (var card in c.PuppetCards)
                {
                    SendCard(session, card);
                    SendTo(card.RequesterId, P(18, _ => { }));
                }
            }
        }
    }

    /// <summary>Tells the partners of a leaving player and drops their invitation.</summary>
    /// <param name="session">The leaving player's session.</param>
    private void OnLeaving(GameSession session)
    {
        if (session.ActiveCharacter is not { } c || session.Account is null)
            return;
        var id = session.Account.LoginId;
        lock (_lock)
        {
            _invites.Remove(id);
            if (c.PuppetHelperId != 0)
                SendTo(c.PuppetHelperId, P(15, w => w.I32(c.PuppetHelperId).I32(id)));
            foreach (var card in c.PuppetCards)
                SendTo(card.RequesterId, P(15, w => w.I32(card.RequesterId).I32(id)));
        }
    }
}
