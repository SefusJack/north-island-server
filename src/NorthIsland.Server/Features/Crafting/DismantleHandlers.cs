using Microsoft.Extensions.Logging;
using NorthIsland.Protocol.Messages.Items;

namespace NorthIsland.Server.Features.Crafting;

/// <summary>Handles the Tear Plier requests, which take the stone out of a piece of bag equipment and give it back to the bag.</summary>
/// <param name="items">The item data.</param>
/// <param name="logger">The log.</param>
public sealed class DismantleHandlers(
    ItemData items,
    ILogger<DismantleHandlers> logger)
{
    /// <summary>Item ids of the Tear Plier (mode 1, several uses) and the Small Tear Plier (mode 2, one use).</summary>
    public const ushort TearPlier = 34152, SmallTearPlier = 34155; // 34152 Tear Plier, 34155 Small Tear Plier

    /// <summary>The highest stone rank the Small Tear Plier can remove.</summary>
    public const int SmallPlierMaxRank = 25;

    /// <summary>Handles <c>23/86</c>: moves the piece's stone to the bag, spends a use of the plier and sends the result.</summary>
    /// <param name="session">The player's session.</param>
    /// <param name="body">The packet body: the equipment bag slot, the plier bag slot, then the mode (1 or 2).</param>
    internal ValueTask OnDismantle(GameSession session, ReadOnlyMemory<byte> body)
    {
        if (session.State != SessionState.InWorld || session.ActiveCharacter is not { } c || body.Length < 3 || session.Battle is not null)
            return ValueTask.CompletedTask;
        var (gearSlot, plierSlot, mode) = (body.Span[0], body.Span[1], body.Span[2]);
        var gear = c.Bag.Find(b => b.Slot == gearSlot);
        var plier = c.Bag.Find(b => b.Slot == plierSlot);
        var stone = gear is null || gear.Insert == 0 ? null : items[gear.Insert];
        if (gear is null || plier is null || stone is null || (mode == 1 ? plier.ItemId != TearPlier : mode != 2 || plier.ItemId != SmallTearPlier)
            || (mode == 2 && stone.ShopColumn > SmallPlierMaxRank) || Bag.FreeSlot(c, items, gear.Insert) == 0)
        {
            logger.LogInformation(
                "[{Session}] {Name}: dismantle refused (gear slot {Gear}, plier slot {Plier}, mode {Mode})",
                session.Id,
                c.Name,
                gearSlot,
                plierSlot,
                mode);
            return ValueTask.CompletedTask;
        }
        var stoneId = gear.Insert;
        gear.Insert = 0;
        foreach (var (stack, added) in Bag.Add(c, items, stoneId, 1))
            session.Send(ItemMessages.BagAdd(new BagEntry { Slot = stack.Slot, ItemId = stack.ItemId, Count = added, Wear = stack.Wear }));
        if (plier.ItemId == TearPlier && items[TearPlier] is { } record)
            UseLimit.Spend(session, c, plier, record);
        else
        {
            plier.Count--;
            session.Send(ItemMessages.BagSlotDecrease(plier.Slot, 1));
            if (plier.Count == 0)
                c.Bag.Remove(plier);
        }
        session.Send(DismantleMessages.Result(gear.Slot, gear.Wear));
        logger.LogInformation("[{Session}] {Name}: {Item} loses its stone {Stone}", session.Id, c.Name, items[gear.ItemId]?.Name.Trim(), stoneId);
        return ValueTask.CompletedTask;
    }
}
