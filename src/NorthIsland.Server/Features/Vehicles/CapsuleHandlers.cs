using Microsoft.Extensions.Logging;
using NorthIsland.Protocol.Messages.Items;

namespace NorthIsland.Server.Features.Vehicles;

/// <summary>Handles the Space Capsule: turns a vehicle in the bag into its capsule item.</summary>
/// <param name="items">The item records; a vehicle's capsule id is read from its record.</param>
/// <param name="logger">Logger for the handler's messages.</param>
public sealed class CapsuleHandlers(
    ItemData items,
    ILogger<CapsuleHandlers> logger)
{
    /// <summary>Item id of the Space Capsule, the item that is used up when a vehicle is stored.</summary>
    public const ushort SpaceCapsule = 34270; // Space Capsule

    /// <summary>Tells whether an item id is one of the vehicle capsules (ids 48019 to 48034).</summary>
    /// <param name="id">The item id.</param>
    /// <returns>True when the id is a vehicle capsule.</returns>
    public static bool IsVehicleCapsule(ushort id) => id is >= 48019 and <= 48034; // 48019 Air Balloon Capsule, 48034 Space Craft Capsule

    /// <summary>Stores a vehicle from the bag as its capsule, using up one Space Capsule (client packet <c>23/125</c>).</summary>
    /// <param name="session">The player's session.</param>
    /// <param name="body">The bag slot of the Space Capsule and the bag slot of the vehicle; with fewer than two bytes the first of each is taken.</param>
    internal ValueTask OnConfirm(GameSession session, ReadOnlyMemory<byte> body)
    {
        if (session.State != SessionState.InWorld || session.ActiveCharacter is not { } c || session.Battle is not null)
            return ValueTask.CompletedTask;
        var named = body.Length >= 2;
        bool IsVehicle(BagItem b) => items[b.ItemId] is { Type: 39 } r && r.Words429.Length > 1 && r.Words429[1] != 0
            && !IsVehicleCapsule(b.ItemId) && session.Vehicle?.BagSlot != b.Slot;
        var tool = named ? c.Bag.Find(b => b.Slot == body.Span[0] && b.ItemId == SpaceCapsule) // FCapsuleSlot
            : c.Bag.Where(b => b.ItemId == SpaceCapsule).OrderBy(b => b.Slot).FirstOrDefault();
        var vehicle = named ? c.Bag.Find(b => b.Slot == body.Span[1] && IsVehicle(b)) : c.Bag.OrderBy(b => b.Slot).FirstOrDefault(IsVehicle); // FItemSlot
        if (tool is null || vehicle is null || items[vehicle.ItemId] is not { } record)
        {
            logger.LogInformation("[{Session}] {Name}: space capsule without a capsule or a vehicle in the bag", session.Id, c.Name);
            return ValueTask.CompletedTask;
        }
        var capsuleId = record.Words429[1];
        var (slot, wear) = (vehicle.Slot, vehicle.Wear);
        c.Bag.Remove(vehicle);
        if (items[capsuleId] is null || Bag.FreeSlot(c, items, capsuleId) == 0)
        {
            c.Bag.Add(vehicle);
            return ValueTask.CompletedTask;
        }
        session.Send(ItemMessages.BagSlotDecrease(slot, 1));
        var placed = Bag.Fits(c, items, capsuleId, slot) ? slot : Bag.FreeSlot(c, items, capsuleId);
        c.Bag.Add(new BagItem { Slot = placed, ItemId = capsuleId, Count = 1, Wear = wear });
        session.Send(ItemMessages.BagAdd(new BagEntry { Slot = placed, ItemId = capsuleId, Count = 1, Wear = wear }));
        tool.Count--;
        session.Send(ItemMessages.BagSlotDecrease(tool.Slot, 1));
        if (tool.Count == 0)
            c.Bag.Remove(tool);
        logger.LogInformation("[{Session}] {Name}: {Vehicle} is stored as {Capsule}", session.Id, c.Name, record.Name.Trim(), items[capsuleId]!.Name.Trim());
        return ValueTask.CompletedTask;
    }
}
