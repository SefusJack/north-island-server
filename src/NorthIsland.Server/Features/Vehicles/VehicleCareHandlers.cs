using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NorthIsland.GameData.Items;
using NorthIsland.Protocol.Messages.Items;
using NorthIsland.Protocol.Messages.World;

namespace NorthIsland.Server.Features.Vehicles;

/// <summary>Looks after the wear and fuel of the ridden vehicle: fuel items, refuelling and the once-a-minute tick.</summary>
/// <param name="items">The item records.</param>
/// <param name="world">The online sessions, used to tell the map when a rider is put down.</param>
/// <param name="options">The vehicle options (fuel and wear rates).</param>
/// <param name="logger">Logger for the handler's messages.</param>
public sealed class VehicleCareHandlers(
    ItemData items,
    WorldState world,
    IOptions<VehicleFeatureOptions> options,
    ILogger<VehicleCareHandlers> logger)
{
    /// <summary>Gives the wear at which a vehicle is worn out.</summary>
    /// <param name="vehicle">The vehicle's item record.</param>
    /// <returns>The maximum wear.</returns>
    public static int MaxWear(ItemRecord vehicle) => (int)Math.Ceiling(Math.Pow(Math.Max(1, (int)vehicle.ShopColumn), 1.3)) * 1800;

    /// <summary>Sends the vehicle's wear and fuel to its owner.</summary>
    /// <param name="session">The player's session.</param>
    /// <param name="vehicle">The vehicle's bag stack.</param>
    public static void SendState(GameSession session, BagItem vehicle) =>
        session.Send(VehicleMessages.Wear(vehicle.Slot, session.Account!.LoginId, vehicle.Used, vehicle.Extra));

    /// <summary>Uses a fuel item from the bag on the ridden vehicle and takes one from the stack when it was accepted.</summary>
    /// <param name="session">The player's session.</param>
    /// <param name="c">The player's character.</param>
    /// <param name="stack">The bag stack of the item.</param>
    /// <param name="item">The item's record.</param>
    /// <returns>True when the item is a fuel item (even if the vehicle did not take it), so the caller stops handling it.</returns>
    public bool TryUseFuel(GameSession session, Character c, BagItem stack, ItemRecord item)
    {
        if (item.SubType != 121)
            return false;
        if (!Refuel(session, c, item.Id))
            return true;
        stack.Count--;
        session.Send(ItemMessages.BagSlotDecrease(stack.Slot, 1));
        if (stack.Count == 0)
            c.Bag.Remove(stack);
        session.Send(ItemMessages.UsedSound());
        return true;
    }

    /// <summary>Adds fuel to the ridden vehicle when its record names that fuel and the tank is not full.</summary>
    /// <param name="session">The player's session.</param>
    /// <param name="c">The player's character.</param>
    /// <param name="fuelItem">The id of the fuel item.</param>
    /// <returns>True when fuel was added.</returns>
    public bool Refuel(GameSession session, Character c, ushort fuelItem)
    {
        if (session.Vehicle is not { Riding: true } riding
            || c.Bag.Find(b => b.Slot == riding.BagSlot) is not { } vehicle
            || items[vehicle.ItemId] is not { } record
            || (record.Ints42.Length < 2 || (record.Ints42[0] != fuelItem && record.Ints42[1] != fuelItem)))
            return false;
        var max = Math.Max(1, options.Value.Vehicles.FuelMax);
        if (vehicle.Extra >= max)
            return false;
        vehicle.Extra = (ushort)Math.Min(max, vehicle.Extra + Math.Max(1, options.Value.Vehicles.FuelPerItem));
        SendState(session, vehicle);
        logger.LogInformation("[{Session}] {Name} refuels {Vehicle}: {Fuel}", session.Id, c.Name, record.Name.Trim(), vehicle.Extra);
        return true;
    }

    /// <summary>
    /// Applies one minute of fuel use and wear to the ridden vehicle and puts the rider down when the fuel is gone
    /// or the vehicle is worn out.
    /// </summary>
    /// <param name="session">The player's session.</param>
    public void Tick(GameSession session)
    {
        var o = options.Value.Vehicles;
        if (session.ActiveCharacter is not { } c || session.Vehicle is not { Riding: true } riding || (o.FuelPerMinute <= 0 && o.WearPerMinute <= 0)
            || c.Bag.Find(b => b.Slot == riding.BagSlot) is not { } vehicle || items[vehicle.ItemId] is not { } record)
            return;
        var wear = o.WearPerMinute;
        vehicle.Used = (int)Math.Min(MaxWear(record), vehicle.Used + Math.Round(wear));
        if (o.FuelPerMinute > 0)
            vehicle.Extra = (ushort)Math.Max(0, vehicle.Extra - (int)Math.Ceiling(o.FuelPerMinute));
        SendState(session, vehicle);
        var dry = o.FuelPerMinute > 0 && vehicle.Extra == 0;
        var worn = wear > 0 && vehicle.Used >= MaxWear(record);
        if (!dry && !worn)
            return;
        session.Vehicle = null;
        var off = VehicleMessages.Dismounted(vehicle.Slot, session.Account!.LoginId);
        session.Send(off);
        world.Broadcast(session, off, sameMapOnly: true);
        if (dry)
            session.Send(VehicleMessages.NoFuel());
        logger.LogInformation("[{Session}] {Name}'s {Vehicle} stops: {Why}", session.Id, c.Name, record.Name.Trim(), dry ? "no fuel" : "worn out");
    }
}
