using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NorthIsland.GameData.Items;
using NorthIsland.Protocol.IO;
using NorthIsland.Protocol.Messages.Items;
using NorthIsland.Protocol.Opcodes;

namespace NorthIsland.Server.Features.Vehicles;

/// <summary>Handles the Maintenance Man's vehicle repair, which sets a vehicle's wear back to 0 for mall points.</summary>
/// <param name="items">The item records.</param>
/// <param name="options">The vehicle options; they hold the repair price.</param>
/// <param name="logger">Logger for the handler's messages.</param>
public sealed class VehicleRepairHandlers(
    ItemData items,
    IOptions<VehicleFeatureOptions> options,
    ILogger<VehicleRepairHandlers> logger)
{
    /// <summary>Opens the repair window on the client.</summary>
    /// <param name="session">The player's session.</param>
    public static void Open(GameSession session) => session.Send(Result(0));

    /// <summary>Builds the repair result packet; the code 2 also carries the remaining wear.</summary>
    /// <param name="code">0 opens the window, 2 repaired, 3 refused, 4 not enough mall points.</param>
    /// <param name="wear">The wear sent with code 2.</param>
    /// <returns>The packet.</returns>
    private static byte[] Result(byte code, int wear = 0) =>
        code == 2 // repaired
            ? new PacketWriter(Action23.Code, Action23.VehicleRepairResult).U8(2).I32(wear).ToArray() // repaired
            : new PacketWriter(Action23.Code, Action23.VehicleRepairResult).U8(code).ToArray();

    /// <summary>Gives the mall points a repair of the vehicle costs: the points per rank times its rank, at least 1.</summary>
    /// <param name="vehicle">The vehicle's item record.</param>
    /// <returns>The cost in mall points.</returns>
    public int Cost(ItemRecord vehicle) => (int)Math.Max(1, Math.Ceiling(options.Value.Vehicles.RepairPointsPerRank * Math.Max(1, (int)vehicle.ShopColumn)));

    /// <summary>Repairs the vehicle in a bag slot when it is worn, not in use and the player can pay.</summary>
    /// <param name="session">The player's session.</param>
    /// <param name="body">The bag slot of the vehicle.</param>
    internal ValueTask OnRepair(GameSession session, ReadOnlyMemory<byte> body)
    {
        if (session.State != SessionState.InWorld || session.ActiveCharacter is not { } c || session.Account is null || body.Length < 1)
            return ValueTask.CompletedTask;
        var stack = c.Bag.Find(b => b.Slot == body.Span[0]); // bagSlot
        if (stack is null
            || !ParkingHandlers.IsVehicle(stack.ItemId)
            || items[stack.ItemId] is not { } record
            || stack.Used <= 0
            || session.Vehicle?.BagSlot == stack.Slot)
        {
            session.Send(Result(3)); // refused
            return ValueTask.CompletedTask;
        }
        var cost = Cost(record);
        if (session.Account.MallPoints < cost)
        {
            session.Send(Result(4)); // not enough mall points
            return ValueTask.CompletedTask;
        }
        session.Account.MallPoints -= cost;
        session.Send(MallMessages.Points(session.Account.MallPoints));
        stack.Used = 0;
        session.Send(Result(2, 0)); // repaired
        logger.LogInformation("[{Session}] {Name} repairs {Vehicle} for {Cost} points", session.Id, c.Name, record.Name.Trim(), cost);
        return ValueTask.CompletedTask;
    }
}
