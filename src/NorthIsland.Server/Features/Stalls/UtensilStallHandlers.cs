using System.Buffers.Binary;
using Microsoft.Extensions.Logging;
using NorthIsland.Protocol.IO;
using NorthIsland.Protocol.Messages.Items;
using NorthIsland.Protocol.Messages.Player;
using NorthIsland.Protocol.Opcodes;

namespace NorthIsland.Server.Features.Stalls;

/// <summary>Handles large item stalls: opening, closing, browsing and buying.</summary>
/// <param name="world">World state.</param>
/// <param name="items">Item data.</param>
/// <param name="logger">Logger.</param>
public sealed class UtensilStallHandlers(
    WorldState world,
    ItemData items,
    ILogger<UtensilStallHandlers> logger)
{
    /// <summary>Most bag stacks one stall can offer.</summary>
    public const int MaxItems = 3;

    /// <summary>Closes the player's large item stall and tells them and the map; nothing happens when none is open.</summary>
    /// <param name="session">The owner's session.</param>
    /// <param name="world">World state, for the broadcast.</param>
    public static void Close(GameSession session, WorldState world)
    {
        if (session.Stalls.Utensils is null || session.Account is null)
            return;
        session.Stalls.Utensils = null;
        var packet = StallMessages.UtensilClosed(session.Account.LoginId);
        session.Send(packet);
        world.Broadcast(session, packet, sameMapOnly: true);
    }

    /// <summary>Tells whether a bag stack may go into the stall: it takes more than one cell or is a vehicle, and flag 2 is not set.</summary>
    /// <param name="stack">The bag stack.</param>
    /// <returns>True when the stack can be offered.</returns>
    private bool Stallable(BagItem stack) =>
        items[stack.ItemId] is { } r && ((r.CellWidth > 1 || r.CellHeight > 1) || r.EquipSlot == 7) && (r.Flags & 2) == 0;

    /// <summary>Opens a large item stall from the player's list of priced bag slots.</summary>
    /// <param name="session">The player's session.</param>
    /// <param name="body">The packet body: name, optional sign byte, then pairs of bag slot and price.</param>
    internal ValueTask OnOpen(GameSession session, ReadOnlyMemory<byte> body)
    {
        if (InWorld(session) is not { } character
            || session.Stalls.Utensils is not null
            || session.Stalls.Items is not null
            || session.Stalls.Pets is not null
            || session.Battle is not null
            || body.Length < 2)
            return ValueTask.CompletedTask;
        var r = new PacketReader(body.Span);
        var stall = new UtensilStall { Name = r.PStr8(), Sign = r.Remaining > 0 ? r.U8() : (byte)0 };
        while (r.Remaining >= 5 && stall.Prices.Count < MaxItems)
        {
            var (slot, price) = (r.U8(), r.I32());
            if (price > 0 && character.Bag.Find(b => b.Slot == slot) is { } stack && Stallable(stack) && session.Vehicle?.BagSlot != slot)
                stall.Prices[slot] = price;
        }
        if (stall.Prices.Count == 0 || stall.Name.Length == 0)
        {
            logger.LogWarning("[{Session}] {Name}: large-item stall refused (no priced goods or no name)", session.Id, character.Name);
            return ValueTask.CompletedTask;
        }
        session.Stalls.Utensils = stall;
        session.Send(StallMessages.UtensilOpened(stall.Name, stall.Sign, stall.Prices.Select(p => (p.Key, p.Value))));
        world.Broadcast(session, StallMessages.UtensilList([(session.Account!.LoginId, stall.Name, stall.Sign)]), sameMapOnly: true);
        logger.LogInformation(
            "[{Session}] {Name} opens large-item stall '{Stall}' with {Count} piece(s)",
            session.Id,
            character.Name,
            stall.Name,
            stall.Prices.Count);
        return ValueTask.CompletedTask;
    }

    /// <summary>Closes the player's large item stall.</summary>
    /// <param name="session">The player's session.</param>
    /// <param name="body">The packet body; not read.</param>
    internal ValueTask OnClose(GameSession session, ReadOnlyMemory<byte> body)
    {
        Close(session, world);
        return ValueTask.CompletedTask;
    }

    /// <summary>Sends the goods on offer in a stall, or a closed notice when there is none.</summary>
    /// <param name="session">The browsing player's session.</param>
    /// <param name="body">The packet body: the owner's login id.</param>
    internal ValueTask OnBrowse(GameSession session, ReadOnlyMemory<byte> body)
    {
        if (InWorld(session) is null || body.Length < 4)
            return ValueTask.CompletedTask;
        var ownerId = BinaryPrimitives.ReadInt32LittleEndian(body.Span);
        if (world.SessionOf(ownerId) is not { } owner || owner.Stalls.Utensils is not { } stall || owner.ActiveCharacter is not { } seller)
        {
            session.Send(StallMessages.UtensilClosed(ownerId));
            return ValueTask.CompletedTask;
        }
        session.Send(Offer(ownerId, stall, seller));
        return ValueTask.CompletedTask;
    }

    /// <summary>Buys goods from a stall: checks each line, gold and bag room, then moves the goods and the gold.</summary>
    /// <param name="session">The buyer's session.</param>
    /// <param name="body">The packet body: the owner's login id, then pairs of bag slot and count.</param>
    internal ValueTask OnBuy(GameSession session, ReadOnlyMemory<byte> body)
    {
        if (InWorld(session) is not { } buyer || body.Length < 6)
            return ValueTask.CompletedTask;
        var ownerId = BinaryPrimitives.ReadInt32LittleEndian(body.Span);
        var lines = new List<(byte Slot, byte Count)>();
        for (var i = 4; i + 1 < body.Length; i += 2)
            lines.Add((body.Span[i], body.Span[i + 1])); // Count
        if (world.SessionOf(ownerId) is not { } owner
            || ReferenceEquals(owner, session)
            || owner.Stalls.Utensils is not { } stall
            || owner.ActiveCharacter is not { } seller
            || lines.Count == 0 || lines.Select(l => l.Slot).Distinct().Count() != lines.Count)
            return ValueTask.CompletedTask;

        lock (stall)
        {
            long total = 0;
            var trial = buyer.Bag.Select(b => GridStore.Copy(b, b.Slot, b.Count)).ToList();
            foreach (var (slot, count) in lines)
            {
                if (!stall.Prices.TryGetValue(slot, out var price) || seller.Bag.Find(b => b.Slot == slot) is not { } stack || count == 0 || count > stack.Count
                    || (!Bag.IsStackable(items[stack.ItemId]) && count != 1) || Give(trial, stack, count) == 0)
                {
                    session.Send(Offer(ownerId, stall, seller));
                    return ValueTask.CompletedTask;
                }
                total += (long)price * count;
            }
            if (total > buyer.Gold || (long)seller.Gold + total > BankHandlers.MaxGold)
            {
                logger.LogInformation(
                    "[{Session}] {Buyer} cannot buy from {Seller}'s large-item stall: {Why}",
                    session.Id,
                    buyer.Name,
                    seller.Name,
                    total > buyer.Gold ? "not enough gold" : "the seller's purse is full");
                return ValueTask.CompletedTask;
            }
            foreach (var (slot, count) in lines)
            {
                var stack = seller.Bag.Find(b => b.Slot == slot)!;
                var price = stall.Prices[slot];
                var added = GiveReal(buyer, stack, count);
                foreach (var entry in added)
                    session.Send(ItemMessages.BagAdd(entry));
                stack.Count -= count;
                owner.Send(ItemMessages.BagSlotDecrease(slot, count));
                owner.Send(new PacketWriter(Action23.Code, Action23.UtensilStallSale).U8(1).U16(stack.ItemId).U8(count).I32(price * count).U8(slot).ToArray());
                if (stack.Count == 0)
                {
                    seller.Bag.Remove(stack);
                    stall.Prices.Remove(slot);
                }
                logger.LogInformation(
                    "[{Session}] {Buyer} buys {Item} x{Count} from {Seller}'s large-item stall for {Gold}",
                    session.Id,
                    buyer.Name,
                    items[stack.ItemId]?.Name.Trim(),
                    count,
                    seller.Name,
                    price * count);
            }
            buyer.Gold -= (int)total;
            session.Send(PlayerMessages.MoneyChange(-(int)total));
            seller.Gold += (int)total;
            owner.Send(PlayerMessages.MoneyChange((int)total));
            if (stall.Prices.Count == 0)
                Close(owner, world);
            else
                session.Send(Offer(ownerId, stall, seller));
        }
        return ValueTask.CompletedTask;
    }

    /// <summary>Checks that a stack fits in a trial bag and adds it there.</summary>
    /// <param name="bag">The trial bag, a copy of the buyer's.</param>
    /// <param name="from">The seller's stack.</param>
    /// <param name="count">How many to give.</param>
    /// <returns>The count given, or 0 when it does not fit.</returns>
    private int Give(List<BagItem> bag, BagItem from, byte count)
    {
        if (Bag.IsStackable(items[from.ItemId]))
            return Bag.Add(bag, items, from.ItemId, count, from.Wear, from.Insert, from.Fusion, from.Pose, from.BoxItem).Sum(x => x.Added) == count ? count : 0;
        var slot = Bag.FreeSlot(bag, items, from.ItemId);
        if (slot == 0)
            return 0;
        bag.Add(GridStore.Copy(from, slot, 1));
        return 1;
    }

    /// <summary>Puts the goods into the buyer's real bag.</summary>
    /// <param name="buyer">The buyer's character.</param>
    /// <param name="from">The seller's stack.</param>
    /// <param name="count">How many to give.</param>
    /// <returns>The bag entries to send to the buyer.</returns>
    private List<BagEntry> GiveReal(Character buyer, BagItem from, byte count)
    {
        var entries = new List<BagEntry>();
        if (Bag.IsStackable(items[from.ItemId]))
        {
            foreach (var (stack, added) in Bag.Add(buyer.Bag, items, from.ItemId, count, from.Wear, from.Insert, from.Fusion, from.Pose, from.BoxItem))
                entries.Add(Entry(stack, added));
            return entries;
        }
        var slot = Bag.FreeSlot(buyer.Bag, items, from.ItemId);
        var copy = GridStore.Copy(from, slot, 1);
        buyer.Bag.Add(copy);
        entries.Add(Entry(copy, 1));
        return entries;
    }

    /// <summary>Describes a bag stack as a bag entry.</summary>
    /// <param name="b">The bag stack.</param>
    /// <param name="count">The count to show.</param>
    /// <returns>The bag entry.</returns>
    private static BagEntry Entry(BagItem b, byte count) => new()
    {
        Slot = b.Slot,
        ItemId = b.ItemId,
        Count = count,
        Wear = b.Wear,
        Insert = b.Insert,
        FusionLevel = b.Fusion,
        PoseItemId = b.Pose,
        BoxItemId = b.BoxItem,
        UsedAmount = b.Used,
        ExtraAmount = b.Extra,
    };

    /// <summary>Builds the offer packet of a stall with the goods that are still in the seller's bag.</summary>
    /// <param name="ownerId">The owner's login id.</param>
    /// <param name="stall">The stall.</param>
    /// <param name="seller">The owner's character.</param>
    /// <returns>The packet bytes.</returns>
    private static byte[] Offer(int ownerId, UtensilStall stall, Character seller)
    {
        var w = new PacketWriter(Action23.Code, Action23.UtensilStallOffer).I32(ownerId).Bytes(new byte[12]);
        foreach (var (slot, price) in stall.Prices.OrderBy(p => p.Key))
            if (seller.Bag.Find(b => b.Slot == slot) is { } s)
                w.U8(slot).U16(s.ItemId).U8(s.Count).U8(s.Wear).I32(price).I32(s.Used).U16(s.Extra);
        return w.ToArray();
    }

    /// <summary>Gives the active character of a session that is in the world.</summary>
    /// <param name="session">The player's session.</param>
    /// <returns>The character, or null when the session is not in the world.</returns>
    private static Character? InWorld(GameSession session)
    {
        var character = session.ActiveCharacter;
        return session.State == SessionState.InWorld && character is not null && session.Account is not null ? character : null;
    }
}
