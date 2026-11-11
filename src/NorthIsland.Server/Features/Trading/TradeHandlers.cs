using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NorthIsland.Protocol.IO;
using NorthIsland.Protocol.Messages.Items;
using NorthIsland.Protocol.Messages.Player;
using NorthIsland.Protocol.Opcodes;

namespace NorthIsland.Server.Features.Trading;

/// <summary>
/// Serves the item trade and the large-item trade: two players on the same map each offer gold and bag stacks, and both must confirm.
/// </summary>
/// <param name="world">The online sessions.</param>
/// <param name="items">The item records.</param>
/// <param name="options">The trade options.</param>
/// <param name="logger">The logger.</param>
public sealed class TradeHandlers(
    WorldState world,
    ItemData items,
    IOptions<TradeOptions> options,
    ILogger<TradeHandlers> logger)
{
    /// <summary>Cancels the item trade the session is in, if any, and tells the partner it was cancelled.</summary>
    /// <param name="session">The player's session.</param>
    public static void Abort(GameSession session)
    {
        if (session.Trades.Items is not { } trade)
            return;
        lock (trade.Lock)
        {
            if (trade.Closed)
                return;
            Close(trade);
        }
        trade.PartnerOf(session).Session.Send(ResultPacket(trade, TradeResult.Cancelled, null));
    }

    /// <summary>
    /// Builds the result packet for one side; a large trade gets the large result codes, where a too-many-items result names which side lacked room.
    /// </summary>
    /// <param name="trade">The trade.</param>
    /// <param name="result">The result.</param>
    /// <param name="side">The side the packet is for, or null when it does not matter.</param>
    /// <returns>The packet.</returns>
    private static byte[] ResultPacket(Trade trade, TradeResult result, TradeSide? side)
    {
        if (!trade.Large)
            return TradeMessages.Result(result);
        return TradeMessages.LargeResult(result switch
        {
            TradeResult.TooManyItems => trade.NoSpace is null ? (byte)10 : ReferenceEquals(trade.NoSpace, side) ? (byte)10 : (byte)9, // lacking side 10, else 9
            TradeResult.AlreadyTrading => (byte)2, // already trading: large code 2
            _ => (byte)result, // any other result: its own value as the code
        });
    }

    /// <summary>Opens an item trade with another player (<c>25/1</c>).</summary>
    /// <param name="session">The player's session.</param>
    /// <param name="body">The packet body: i32 login id of the other player.</param>
    internal ValueTask OnRequest(GameSession session, ReadOnlyMemory<byte> body) => OnRequest(session, body, large: false);

    /// <summary>
    /// Opens an item or large-item trade with another player (<c>25/1</c>, <c>25/40</c>), or answers with the reason it is refused.
    /// </summary>
    /// <param name="session">The player's session.</param>
    /// <param name="body">The packet body: i32 login id of the other player.</param>
    /// <param name="large">True for a large-item trade.</param>
    internal ValueTask OnRequest(GameSession session, ReadOnlyMemory<byte> body, bool large)
    {
        if (InWorld(session) is not { } character || body.Length < 4)
            return ValueTask.CompletedTask;
        var targetId = System.Buffers.Binary.BinaryPrimitives.ReadInt32LittleEndian(body.Span);
        TradeResult? refusal;
        var hours = options.Value.TradeOnlineHours;
        if (!OnlineTime.Enough(session, hours, GameClock.UtcNow))
        {
            logger.LogInformation("[{Session}] {Name}: trade request refused, less than {Hours} h online", session.Id, character.Name, hours);
            return ValueTask.CompletedTask;
        }
        if (session.Trades.Items is not null)
            refusal = TradeResult.AlreadyTrading;
        else if (world.SessionOf(targetId) is not { } target || target.ActiveCharacter is null || ReferenceEquals(target, session))
            refusal = TradeResult.TargetOffline;
        else if (target.ActiveCharacter.MapId != character.MapId)
            refusal = TradeResult.DifferentScene;
        else if (target.Trades.Items is not null
            || target.Event is not null
            || (large && (session.Stalls.Any || target.Stalls.Any))
            || !OnlineTime.Enough(target, hours, GameClock.UtcNow))
            refusal = TradeResult.TargetBusy;
        else if (large && (character.Level < 10 || target.ActiveCharacter.Level < 10))
        {
            session.Send(new PacketWriter(Action25.Code, Action25.LevelNotice).ToArray());
            return ValueTask.CompletedTask;
        }
        else
        {
            var trade = new Trade(session, target) { Large = large };
            session.Trades.Items = trade;
            target.Trades.Items = trade;
            session.Send(large ? TradeMessages.LargeStart(target.Account!.LoginId) : TradeMessages.Start(target.Account!.LoginId));
            target.Send(large ? TradeMessages.LargeStart(session.Account!.LoginId) : TradeMessages.Start(session.Account!.LoginId));
            logger.LogInformation("[{Session}] trade opened: {A} with {B}", session.Id, character.Name, target.ActiveCharacter.Name);
            return ValueTask.CompletedTask;
        }
        session.Send(
            large
                ? TradeMessages.LargeResult(refusal.Value == TradeResult.AlreadyTrading ? (byte)2 : (byte)refusal.Value)
                : TradeMessages.Result(refusal.Value));
        logger.LogInformation("[{Session}] trade request to {Target} refused: {Reason}", session.Id, targetId, refusal);
        return ValueTask.CompletedTask;
    }

    /// <summary>
    /// Records the player's offer of gold and bag stacks and shows it to the partner (<c>25/2</c>, <c>25/41</c>). An offer they cannot make cancels
    /// the trade.
    /// </summary>
    /// <param name="session">The player's session.</param>
    /// <param name="body">The packet body: the offered gold and bag slots.</param>
    internal ValueTask OnOffer(GameSession session, ReadOnlyMemory<byte> body)
    {
        if (InWorld(session) is not { } character || session.Trades.Items is not { } trade || body.Length < 4)
            return ValueTask.CompletedTask;
        var offer = TradeOffer.Parse(body.Span);
        var slots = offer.BagSlots.Distinct().ToList();
        var stacks = slots.Select(s => character.Bag.Find(b => b.Slot == s)).ToList();
        if (offer.Gold < 0 || offer.Gold > character.Gold || stacks.Any(s => s is null || Bag.IsNoTrade(items, s)) || slots.Any(s => session.Vehicle?.BagSlot == s))
        {
            logger.LogWarning(
                "[{Session}] trade offer refused: gold {Gold} (has {Has}), slots {Slots}",
                session.Id,
                offer.Gold,
                character.Gold,
                string.Join(',', slots));
            var cancelled = ResultPacket(trade, TradeResult.Cancelled, null);
            Abort(session);
            session.Send(cancelled);
            return ValueTask.CompletedTask;
        }

        GameSession partner;
        lock (trade.Lock)
        {
            if (trade.Closed)
                return ValueTask.CompletedTask;
            var side = trade.Of(session);
            side.Gold = offer.Gold;
            side.BagSlots.Clear();
            side.BagSlots.AddRange(slots);
            side.Offered = true;
            side.Confirmed = false;
            partner = trade.PartnerOf(session).Session;
        }
        partner.Send(trade.Large
            ? TradeMessages.LargeOffer(offer.Gold, stacks.Select(s => (s!.ItemId, s.Used, s.Extra, s.Wear)))
            : TradeMessages.PartnerOffer(offer.Gold, stacks.Select(s => new TradeItem(s!.ItemId, s.Count, s.Wear))));
        logger.LogInformation("[{Session}] {Name} offers {Gold} gold and {Items}", session.Id, character.Name, offer.Gold,
            stacks.Count == 0 ? "no items" : string.Join(", ", stacks.Select(s => $"{items[s!.ItemId]?.Name} x{s.Count}")));
        return ValueTask.CompletedTask;
    }

    /// <summary>
    /// Confirms or cancels the trade (<c>25/3</c>, <c>25/42</c>). When both sides have confirmed the trade is carried out and both are told the
    /// result.
    /// </summary>
    /// <param name="session">The player's session.</param>
    /// <param name="body">The packet body: u8, 1 to confirm, anything else to cancel.</param>
    internal ValueTask OnDecide(GameSession session, ReadOnlyMemory<byte> body)
    {
        if (InWorld(session) is null || session.Trades.Items is not { } trade || body.Length < 1)
            return ValueTask.CompletedTask;
        if (body.Span[0] != 1)
        {
            Abort(session);
            logger.LogInformation("[{Session}] trade cancelled", session.Id);
            return ValueTask.CompletedTask;
        }

        TradeResult? result = null;
        lock (trade.Lock)
        {
            if (trade.Closed)
                return ValueTask.CompletedTask;
            var side = trade.Of(session);
            if (!side.Offered)
                return ValueTask.CompletedTask;
            side.Confirmed = true;
            if (trade.First.Confirmed && trade.Second.Confirmed)
            {
                result = Execute(trade);
                Close(trade);
            }
        }
        if (result is { } r)
        {
            trade.First.Session.Send(ResultPacket(trade, r, trade.First));
            trade.Second.Session.Send(ResultPacket(trade, r, trade.Second));
        }
        return ValueTask.CompletedTask;
    }

    /// <summary>Carries out a confirmed trade: checks gold, stacks and room on both sides, then moves the stacks and the gold difference.</summary>
    /// <param name="trade">The trade, locked by the caller.</param>
    /// <returns>The result sent to both players.</returns>
    private TradeResult Execute(Trade trade)
    {
        var a = trade.First;
        var b = trade.Second;
        var ca = a.Session.ActiveCharacter!;
        var cb = b.Session.ActiveCharacter!;
        if (a.Gold > ca.Gold || b.Gold > cb.Gold
            || (long)cb.Gold - b.Gold + a.Gold > BankHandlers.MaxGold || (long)ca.Gold - a.Gold + b.Gold > BankHandlers.MaxGold)
            return TradeResult.NoFunds;
        var giveA = Stacks(ca, a.BagSlots);
        var giveB = Stacks(cb, b.BagSlots);
        if (giveA is null || giveB is null)
            return TradeResult.Cancelled;
        var fitsA = Fits(ca, giveA, giveB);
        var fitsB = Fits(cb, giveB, giveA);
        if (!fitsA || !fitsB)
        {
            trade.NoSpace = !fitsA ? a : b;
            return TradeResult.TooManyItems;
        }

        Move(a.Session, ca, giveA, giveB, b.Gold - a.Gold);
        Move(b.Session, cb, giveB, giveA, a.Gold - b.Gold);
        logger.LogInformation("Trade done: {A} gave {GoldA} gold + {ItemsA}; {B} gave {GoldB} gold + {ItemsB}",
            ca.Name, a.Gold, Describe(giveA), cb.Name, b.Gold, Describe(giveB));
        return TradeResult.Success;
    }

    /// <summary>
    /// Applies one side of a trade: removes the stacks given, adds the stacks received and changes the gold, telling the client of each.
    /// </summary>
    /// <param name="session">The player's session.</param>
    /// <param name="character">The character of that side.</param>
    /// <param name="give">The stacks they give away.</param>
    /// <param name="receive">The stacks they get.</param>
    /// <param name="goldChange">The gold added to their purse; negative to take.</param>
    private void Move(
        GameSession session,
        Character character,
        List<BagItem> give,
        List<BagItem> receive,
        int goldChange)
    {
        foreach (var stack in give)
        {
            session.Send(ItemMessages.BagSlotDecrease(stack.Slot, stack.Count));
            character.Bag.Remove(stack);
        }
        foreach (var stack in receive)
        {
            if (!Bag.IsStackable(items[stack.ItemId]))
            {
                var slot = Bag.FreeSlot(character.Bag, items, stack.ItemId);
                var copy = GridStore.Copy(stack, slot, stack.Count);
                character.Bag.Add(copy);
                session.Send(
                    ItemMessages.BagAdd(
                        new BagEntry
                        {
                            Slot = slot,
                            ItemId = copy.ItemId,
                            Count = copy.Count,
                            Wear = copy.Wear,
                            Insert = copy.Insert,
                            FusionLevel = copy.Fusion,
                            PoseItemId = copy.Pose,
                            BoxItemId = copy.BoxItem,
                            UsedAmount = copy.Used,
                            ExtraAmount = copy.Extra
                        }));
                continue;
            }
            foreach (var (bagStack, added) in Bag.Add(
                character,
                items,
                stack.ItemId,
                stack.Count,
                stack.Wear,
                stack.Insert,
                stack.Fusion,
                stack.Pose,
                stack.BoxItem))
                session.Send(
                    ItemMessages.BagAdd(
                        new BagEntry
                        {
                            Slot = bagStack.Slot,
                            ItemId = stack.ItemId,
                            Count = added,
                            Wear = stack.Wear,
                            Insert = stack.Insert,
                            FusionLevel = stack.Fusion,
                            PoseItemId = stack.Pose,
                            BoxItemId = stack.BoxItem
                        }));
        }
        if (goldChange != 0)
        {
            character.Gold += goldChange;
            session.Send(PlayerMessages.MoneyChange(goldChange));
        }
    }

    /// <summary>Tells on a trial copy of the bag whether the received stacks fit once the given stacks are gone.</summary>
    /// <param name="character">The character.</param>
    /// <param name="give">The stacks they give away.</param>
    /// <param name="receive">The stacks they get.</param>
    /// <returns>True when everything fits.</returns>
    private bool Fits(Character character, List<BagItem> give, List<BagItem> receive)
    {
        List<BagItem> trial = [.. character.Bag.Where(b => !give.Contains(b))
            .Select(
                b => new BagItem { Slot = b.Slot, ItemId = b.ItemId, Count = b.Count, Wear = b.Wear, Fusion = b.Fusion, Pose = b.Pose, BoxItem = b.BoxItem })
        ];
        return receive.All(s => Bag.Add(trial, items, s.ItemId, s.Count, s.Wear).Sum(c => c.Added) == s.Count);
    }

    /// <summary>Looks up the bag stacks of the offered slots.</summary>
    /// <param name="character">The character.</param>
    /// <param name="slots">The offered bag slots.</param>
    /// <returns>The stacks, or null when a slot no longer holds one.</returns>
    private static List<BagItem>? Stacks(Character character, List<byte> slots)
    {
        var stacks = slots.Select(s => character.Bag.Find(b => b.Slot == s)).ToList();
        return stacks.Any(s => s is null) ? null : [.. stacks.OfType<BagItem>()];
    }

    /// <summary>Describes stacks as item names with counts, for the log.</summary>
    /// <param name="stacks">The stacks.</param>
    /// <returns>The text, "no items" when empty.</returns>
    private string Describe(List<BagItem> stacks) =>
        stacks.Count == 0 ? "no items" : string.Join(", ", stacks.Select(s => $"{items[s.ItemId]?.Name} x{s.Count}"));

    /// <summary>Marks the trade closed and removes it from both sessions.</summary>
    /// <param name="trade">The trade.</param>
    private static void Close(Trade trade)
    {
        trade.Closed = true;
        trade.First.Session.Trades.Items = null;
        trade.Second.Session.Trades.Items = null;
    }

    /// <summary>Returns the active character when the session is in the world with an account.</summary>
    /// <param name="session">The player's session.</param>
    /// <returns>The active character, or null when the request must be ignored.</returns>
    private static Character? InWorld(GameSession session)
    {
        var character = session.ActiveCharacter;
        return session.State == SessionState.InWorld && character is not null && session.Account is not null ? character : null;
    }
}
