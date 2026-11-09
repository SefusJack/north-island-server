using Microsoft.Extensions.Logging;
using NorthIsland.GameData.Items;
using NorthIsland.Protocol.Messages.Items;
using NorthIsland.Protocol.Messages.Player;

namespace NorthIsland.Server.Features.Items;

/// <summary>Exchange vouchers: a vehicle voucher becomes the vehicle, and the 1,000 Gold Voucher pays 1,000 gold.</summary>
/// <param name="items">Item definitions, to check that the vehicle exists.</param>
/// <param name="logger">Logger.</param>
public sealed class ExchangeVoucherHandlers(
    ItemData items,
    ILogger<ExchangeVoucherHandlers> logger)
{
    /// <summary>The item id of the 1,000 Gold Voucher.</summary>
    public const ushort GoldVoucher = 34005; // 1,000 Gold Voucher

    /// <summary>The most gold a player can carry.</summary>
    public const int MaxGold = 9_999_999;

    /// <summary>The vehicle a voucher is exchanged for.</summary>
    /// <param name="voucher">The voucher's item id.</param>
    /// <returns>The vehicle's item id, 0 when the item is not a vehicle voucher.</returns>
    public static ushort VehicleOf(ushort voucher) => voucher switch
    {
        30045 => 48003, // 30045 Boat Voucher, 48003 Boat
        30046 => 48001, // 30046 Hot-air Balloon Card, 48001 Hot-air Balloon
        30047 => 48002, // 30047 Airship Voucher, 48002 Airship
        30048 => 48004, // 30048 Labor Plane Voucher, 48004 Mighty Plane
        30049 => 48006, // 30049 Steam Ship Voucher, 48006 Steam Ship
        30050 or 30066 => 48007, // 30050 Motorbike Voucher, 30066 Motorbike Ticket, 48007 Motorbike
        30051 or 30065 => 48009, // 30051 Turtle Car Voucher, 30065 Turtle Car Voucher, 48009 Turtle Car
        30052 or 34075 => 48010, // 30052 Raft Voucher, 34075 Raft Voucher, 48010 Raft
        30067 => 48012, // 30067 Yacht Voucher, 48012 Yacht
        30064 => 48013, // 30064 UFO Voucher, 48013 UFO
        _ => 0, // not a vehicle voucher
    };

    /// <summary>Exchanges a voucher: gold for the gold voucher, otherwise a new vehicle if the bag has room and the player is not in a battle.</summary>
    /// <param name="session">The player's session.</param>
    /// <param name="c">The player's character.</param>
    /// <param name="stack">The voucher stack that was used.</param>
    /// <param name="item">The voucher's definition.</param>
    /// <returns>True when the item is a voucher and was handled, even if the exchange was refused; false for any other item.</returns>
    public bool TryUse(GameSession session, Character c, BagItem stack, ItemRecord item)
    {
        if (item.Id == GoldVoucher)
        {
            if ((long)c.Gold + 1000 > MaxGold)
                return true;
            Take(session, c, stack);
            c.Gold += 1000;
            session.Send(PlayerMessages.Money(c.Gold));
            logger.LogInformation("[{Session}] {Name} exchanges a Gold Voucher: {Gold} gold", session.Id, c.Name, c.Gold);
            return true;
        }
        var vehicle = VehicleOf(item.Id);
        if (vehicle == 0)
            return false;
        if (session.Battle is not null || items[vehicle] is null)
            return true;
        var trial = c.Bag.Select(b => GridStore.Copy(b, b.Slot, b.Count)).ToList();
        var copy = trial.Find(b => b.Slot == stack.Slot)!;
        copy.Count--;
        if (copy.Count == 0)
            trial.Remove(copy);
        if (Bag.Add(trial, items, vehicle, 1).Sum(x => x.Added) != 1)
            return true;
        Take(session, c, stack);
        foreach (var (made, added) in Bag.Add(c.Bag, items, vehicle, 1))
            session.Send(ItemMessages.BagAdd(new BagEntry { Slot = made.Slot, ItemId = made.ItemId, Count = added, Wear = made.Wear }));
        logger.LogInformation("[{Session}] {Name} exchanges {Voucher} for {Vehicle}", session.Id, c.Name, item.Name.Trim(), items[vehicle]!.Name.Trim());
        return true;
    }

    /// <summary>Takes one voucher from the stack, removing the stack when it is empty, and tells the client.</summary>
    /// <param name="session">The player's session.</param>
    /// <param name="c">The player's character.</param>
    /// <param name="stack">The voucher stack.</param>
    private static void Take(GameSession session, Character c, BagItem stack)
    {
        stack.Count--;
        if (stack.Count == 0)
            c.Bag.Remove(stack);
        session.Send(ItemMessages.BagSlotDecrease(stack.Slot, 1));
        session.Send(ItemMessages.UsedSound());
    }
}
