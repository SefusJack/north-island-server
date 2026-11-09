using System.Buffers.Binary;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NorthIsland.Protocol.IO;
using NorthIsland.Protocol.Messages.Items;
using NorthIsland.Protocol.Opcodes;

namespace NorthIsland.Server.Features.Vehicles;

/// <summary>Handles the parking lot: four cells where a player leaves vehicles, for a fee in mall points.</summary>
/// <param name="items">The item records.</param>
/// <param name="options">The vehicle options; they hold the parking fee.</param>
/// <param name="logger">Logger for the handler's messages.</param>
public sealed class ParkingHandlers(
    ItemData items,
    IOptions<VehicleFeatureOptions> options,
    ILogger<ParkingHandlers> logger)
{
    /// <summary>The number of parking cells.</summary>
    public const int Cells = 4;

    /// <summary>Tells whether an item id is a vehicle or a vehicle capsule (ids 48001 to 48034).</summary>
    /// <param name="id">The item id.</param>
    /// <returns>True when the id is a vehicle.</returns>
    public static bool IsVehicle(ushort id) => id is >= 48001 and <= 48034; // 48001 Hot-air Balloon, 48034 Space Craft Capsule

    /// <summary>Sends the parked vehicles and opens the parking window.</summary>
    /// <param name="session">The player's session.</param>
    /// <param name="c">The player's character.</param>
    public static void Open(GameSession session, Character c)
    {
        SendList(session, c);
        session.Send(new PacketWriter(Action23.Code, Action23.ParkOpen).U8(1).ToArray());
    }

    /// <summary>Sends the four cells, with the vehicle in each (zeros for an empty cell).</summary>
    /// <param name="session">The player's session.</param>
    /// <param name="c">The player's character.</param>
    private static void SendList(GameSession session, Character c)
    {
        var w = new PacketWriter(Action23.Code, Action23.ParkList);
        for (byte cell = 1; cell <= Cells; cell++)
        {
            var v = c.Parking.Find(p => p.Cell == cell);
            w.I32(v?.ItemId ?? 0).U8(cell).I32(v?.Used ?? 0).U16(v?.Extra ?? 0);
        }
        session.Send(w.ToArray());
    }

    /// <summary>Builds the parking result packet; the codes 1 and 3 also carry an item id.</summary>
    /// <param name="code">1 parked, 2 refused, 3 redeemed, 4 bag full, 5 vehicle in use, 6 not enough mall points.</param>
    /// <param name="item">The item id sent with codes 1 and 3.</param>
    /// <returns>The packet.</returns>
    private static byte[] Result(byte code, int item = 0) =>
        code is 1 or 3 // 1 parked, 3 redeemed
            ? new PacketWriter(Action23.Code, Action23.ParkResult).U8(code).I32(item).ToArray()
            : new PacketWriter(Action23.Code, Action23.ParkResult).U8(code).ToArray();

    /// <summary>Parks a vehicle from the bag in a cell, charging the fee.</summary>
    /// <param name="session">The player's session.</param>
    /// <param name="body">Two bytes that are not read, then the bag slot and the cell.</param>
    internal ValueTask OnPark(GameSession session, ReadOnlyMemory<byte> body)
    {
        if (session.State != SessionState.InWorld || session.ActiveCharacter is not { } c || session.Account is null || body.Length < 4)
            return ValueTask.CompletedTask;
        var (slot, cell) = (body.Span[2], body.Span[3]);
        var stack = c.Bag.Find(b => b.Slot == slot);
        if (stack is null || !IsVehicle(stack.ItemId) || cell is < 1 or > Cells || c.Parking.Exists(p => p.Cell == cell))
        {
            session.Send(Result(2)); // refused
            return ValueTask.CompletedTask;
        }
        if (session.Vehicle is { } v && v.BagSlot == slot)
        {
            session.Send(Result(5)); // vehicle in use
            return ValueTask.CompletedTask;
        }
        var cost = options.Value.Vehicles.ParkPoints;
        if (cost > 0 && session.Account.MallPoints < cost)
        {
            session.Send(Result(6)); // not enough mall points
            return ValueTask.CompletedTask;
        }
        if (cost > 0)
        {
            session.Account.MallPoints -= cost;
            session.Send(MallMessages.Points(session.Account.MallPoints));
        }
        c.Bag.Remove(stack);
        session.Send(ItemMessages.BagSlotDecrease(slot, 1));
        c.Parking.Add(new ParkedVehicle { Cell = cell, ItemId = stack.ItemId, Used = stack.Used, Extra = stack.Extra, Wear = stack.Wear });
        SendList(session, c);
        session.Send(Result(1, stack.ItemId));
        logger.LogInformation("[{Session}] {Name} parks {Item} in cell {Cell}", session.Id, c.Name, items[stack.ItemId]?.Name.Trim(), cell);
        return ValueTask.CompletedTask;
    }

    /// <summary>Gives the vehicle in a cell back to the bag.</summary>
    /// <param name="session">The player's session.</param>
    /// <param name="body">The cell number (u16).</param>
    internal ValueTask OnRedeem(GameSession session, ReadOnlyMemory<byte> body)
    {
        if (session.State != SessionState.InWorld || session.ActiveCharacter is not { } c || body.Length < 2)
            return ValueTask.CompletedTask;
        var cell = (byte)Math.Min(255, (int)BinaryPrimitives.ReadUInt16LittleEndian(body.Span));
        if (c.Parking.Find(p => p.Cell == cell) is not { } parked)
        {
            session.Send(Result(2)); // refused
            return ValueTask.CompletedTask;
        }
        var slot = Bag.FreeSlot(c, items, parked.ItemId);
        if (slot == 0)
        {
            session.Send(Result(4)); // bag full
            return ValueTask.CompletedTask;
        }
        c.Parking.Remove(parked);
        c.Bag.Add(new BagItem { Slot = slot, ItemId = parked.ItemId, Count = 1, Wear = parked.Wear, Used = parked.Used, Extra = parked.Extra });
        session.Send(
            ItemMessages.BagAdd(
                new BagEntry { Slot = slot, ItemId = parked.ItemId, Count = 1, Wear = parked.Wear, UsedAmount = parked.Used, ExtraAmount = parked.Extra }));
        SendList(session, c);
        session.Send(Result(3, parked.ItemId)); // redeemed
        logger.LogInformation("[{Session}] {Name} redeems {Item} from cell {Cell}", session.Id, c.Name, items[parked.ItemId]?.Name.Trim(), cell);
        return ValueTask.CompletedTask;
    }
}
