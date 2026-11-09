using Microsoft.Extensions.Logging;
using NorthIsland.Protocol.IO;
using NorthIsland.Protocol.Messages.Items;

namespace NorthIsland.Server.Features.Items;

/// <summary>Gift cards: sends the card lists to the client and exchanges cards from the bag for the chosen item.</summary>
/// <param name="items">Item definitions, to check that the item given exists and fits.</param>
/// <param name="logger">Logger.</param>
public sealed class GiftCardHandlers(
    ItemData items,
    ILogger<GiftCardHandlers> logger)
{
    /// <summary>The known gift cards by card item id.</summary>
    private readonly IReadOnlyDictionary<ushort, GiftCard> _cards = GiftCards.Load();

    /// <summary>At login, sends the list version of every card.</summary>
    /// <param name="session">The player's session.</param>
    public void SendOnLogin(GameSession session) =>
        session.Send(GiftCardMessages.Versions([.. _cards.Values.OrderBy(x => x.Id).Select(x => (x.Id, GiftCards.Flag))]));

    /// <summary>The card list as the packet carries it.</summary>
    /// <param name="card">The card.</param>
    /// <returns>The packet entry with the card's choices.</returns>
    private static GiftCardEntry Entry(GiftCard card) =>
        new(card.Id, GiftCards.Flag, [.. card.Choices.Select(x => (x.ItemId, x.Count))]);

    /// <summary>Handles the gift card action: sub code 1 opens a card from a bag slot, 2 exchanges cards.</summary>
    /// <param name="session">The player's session.</param>
    /// <param name="body">The packet body after the action and sub code.</param>
    internal ValueTask OnCard(GameSession session, ReadOnlyMemory<byte> body)
    {
        if (session.State != SessionState.InWorld || session.ActiveCharacter is not { } c || body.Length < 1 || session.Battle is not null)
            return ValueTask.CompletedTask;
        var r = new PacketReader(body.Span[1..]);
        if (body.Span[0] == 1 && r.Remaining >= 4)
        {
            var (slot, cardId, cached) = (r.U8(), r.U16(), r.U8());
            if (c.Bag.Find(b => b.Slot == slot)?.ItemId != cardId || !_cards.TryGetValue(cardId, out var card))
            {
                logger.LogInformation("[{Session}] {Name}: gift card {Card} in slot {Slot}: nothing to exchange it for", session.Id, c.Name, cardId, slot);
                return ValueTask.CompletedTask;
            }
            if (cached != GiftCards.Flag)
                session.Send(GiftCardMessages.Cards([Entry(card)]));
            session.Send(GiftCardMessages.Open(slot));
        }
        else if (body.Span[0] == 2 && r.Remaining >= 5)
        {
            var (slot, times, choice, itemId) = (r.U8(), r.U8(), r.U8(), r.U16());
            Exchange(session, c, slot, times, choice, itemId);
        }
        return ValueTask.CompletedTask;
    }

    /// <summary>Exchanges a number of cards of one slot's kind for the chosen item, if the bag has room for all of them.</summary>
    /// <param name="session">The player's session.</param>
    /// <param name="c">The player's character.</param>
    /// <param name="slot">The bag slot of a card.</param>
    /// <param name="times">How many cards to exchange.</param>
    /// <param name="choice">The 1-based position of the chosen item in the card's list.</param>
    /// <param name="itemId">The item id the client chose, which must match the choice.</param>
    private void Exchange(
        GameSession session,
        Character c,
        byte slot,
        byte times,
        byte choice,
        ushort itemId)
    {
        var stack = c.Bag.Find(b => b.Slot == slot);
        if (stack is null || !_cards.TryGetValue(stack.ItemId, out var card) || choice < 1 || choice > card.Choices.Count
            || card.Choices[choice - 1].ItemId != itemId || times < 1 || Bag.Count(c, card.Id) < times)
        {
            logger.LogInformation(
                "[{Session}] {Name}: gift card exchange refused (slot {Slot}, {Times} x, choice {Choice})",
                session.Id,
                c.Name,
                slot,
                times,
                choice);
            return;
        }
        var give = card.Choices[choice - 1];
        var total = give.Count * times;
        var trial = c.Bag.Select(
            b =>
                new BagItem
                {
                    Slot = b.Slot,
                    ItemId = b.ItemId,
                    Count = b.Count,
                    Wear = b.Wear,
                    Insert = b.Insert,
                    Fusion = b.Fusion,
                    Pose = b.Pose,
                    BoxItem = b.BoxItem
                })
            .ToList();
        Bag.Remove(trial, card.Id, times);
        if (items[give.ItemId] is null || Bag.Add(trial, items, give.ItemId, total).Sum(x => x.Added) != total)
        {
            logger.LogInformation("[{Session}] {Name}: no room for {Count} x {Item}", session.Id, c.Name, total, give.ItemId);
            return;
        }
        var left = (int)times;
        foreach (var cardStack in c.Bag.Where(b => b.ItemId == card.Id).OrderBy(b => b.Slot).ToList())
        {
            if (left == 0)
                break;
            var take = (byte)Math.Min(left, cardStack.Count);
            cardStack.Count -= take;
            left -= take;
            if (cardStack.Count == 0)
                c.Bag.Remove(cardStack);
            session.Send(ItemMessages.BagSlotDecrease(cardStack.Slot, take));
        }
        foreach (var (added, count) in Bag.Add(c, items, give.ItemId, total))
            session.Send(ItemMessages.BagAdd(new BagEntry { Slot = added.Slot, ItemId = added.ItemId, Count = count, Wear = added.Wear }));
        session.Send(GiftCardMessages.Used(card.Id, times, give.ItemId, give.Count));
        logger.LogInformation(
            "[{Session}] {Name} exchanges {Times} x {Card} for {Count} x {Item}",
            session.Id,
            c.Name,
            times,
            items[card.Id]?.Name.Trim(),
            total,
            items[give.ItemId]!.Name.Trim());
    }

    /// <summary>Handles the request for card lists: sends the lists of the cards the client names.</summary>
    /// <param name="session">The player's session.</param>
    /// <param name="body">The packet body: a sub code, a count and that many card item ids.</param>
    internal ValueTask OnLists(GameSession session, ReadOnlyMemory<byte> body)
    {
        if (session.State != SessionState.InWorld || body.Length < 3)
            return ValueTask.CompletedTask;
        var r = new PacketReader(body.Span[1..]);
        var n = r.U16();
        var wanted = new List<GiftCardEntry>();
        for (var i = 0; i < n && r.Remaining >= 2; i++)
            if (_cards.TryGetValue(r.U16(), out var card))
                wanted.Add(Entry(card));
        if (wanted.Count > 0)
            session.Send(GiftCardMessages.Cards(wanted));
        return ValueTask.CompletedTask;
    }
}
