using Microsoft.Extensions.Logging;
using NorthIsland.Protocol.Messages.World;

namespace NorthIsland.Server.Features.Vehicles;

/// <summary>Handles placing a vehicle, boarding, getting off and taking it back.</summary>
/// <param name="items">The item records.</param>
/// <param name="world">The online sessions, used to tell the others on the map.</param>
/// <param name="logger">Logger for the handler's messages.</param>
public sealed class VehicleHandlers(
    ItemData items,
    WorldState world,
    ILogger<VehicleHandlers> logger)
{
    /// <summary>Logs that a client saw a vehicle wreck to the end; nothing is answered.</summary>
    /// <param name="session">The player's session.</param>
    /// <param name="body">The login id of the wreck's owner (i32).</param>
    internal ValueTask OnWreckSeen(GameSession session, ReadOnlyMemory<byte> body)
    {
        if (body.Length >= 4)
            logger.LogDebug(
                "[{Session}] the wreck of {Owner} was seen to the end",
                session.Id,
                System.Buffers.Binary.BinaryPrimitives.ReadInt32LittleEndian(body.Span));
        return ValueTask.CompletedTask;
    }

    /// <summary>Handles a click on the vehicle: a rider gets off, a placed vehicle is taken back, and the map is told.</summary>
    /// <param name="session">The player's session.</param>
    /// <param name="body">The packet body after the action and sub code.</param>
    internal ValueTask OnClick(GameSession session, ReadOnlyMemory<byte> body)
    {
        if (VehicleItemOf(session, body) is not { } request)
            return ValueTask.CompletedTask;
        var id = session.Account!.LoginId;
        var riding = session.Vehicle is not { Riding: false };
        if (riding && BreakRaftOnLanding(session, request.BagSlot, request.ItemId))
            return ValueTask.CompletedTask;
        session.Vehicle = null;
        if (riding)
        {
            var packet = VehicleMessages.Dismounted(request.BagSlot, id);
            session.Send(packet);
            world.Broadcast(session, packet, sameMapOnly: true);
        }
        else
        {
            session.Send(VehicleMessages.TakenBack(request.BagSlot));
            world.Broadcast(session, VehicleMessages.Removed(id, request.ItemId), sameMapOnly: true);
        }
        logger.LogInformation("[{Session}] {Name} {Action} {Item}", session.Id, session.ActiveCharacter!.Name,
            riding ? "gets off" : "takes back", request.ItemId);
        return ValueTask.CompletedTask;
    }

    /// <summary>Places the vehicle on the map at the player's position, boarding it at once when asked, and replaces an earlier placed vehicle.</summary>
    /// <param name="session">The player's session.</param>
    /// <param name="body">The packet body after the action and sub code.</param>
    /// <param name="board">True to board the vehicle as well.</param>
    internal ValueTask OnPlace(GameSession session, ReadOnlyMemory<byte> body, bool board)
    {
        if (VehicleItemOf(session, body) is not { } request)
            return ValueTask.CompletedTask;
        var character = session.ActiveCharacter!;
        var id = session.Account!.LoginId;
        if (session.Vehicle is { } old)
            world.Broadcast(session, VehicleMessages.Dismounted(old.BagSlot, id), sameMapOnly: true);

        session.Vehicle = new PlacedVehicle(request.BagSlot, request.ItemId, character.MapId, character.X, character.Y) { Riding = board };
        var placed = VehicleMessages.Placed(request.BagSlot, id, request.ItemId, character.X, character.Y, board);
        session.Send(placed);
        world.Broadcast(session, VehicleMessages.Placed(request.BagSlot, id, request.ItemId, character.X, character.Y), sameMapOnly: true);
        if (board)
        {
            world.Broadcast(session, VehicleMessages.Boarded(request.BagSlot, id, request.ItemId), sameMapOnly: true);
            if (character.Bag.Find(b => b.Slot == request.BagSlot) is { } boarded)
                VehicleCareHandlers.SendState(session, boarded);
        }
        logger.LogInformation("[{Session}] {Name} {Action} {Item} ({ItemName}) at ({X}, {Y})", session.Id, character.Name,
            board ? "places and boards" : "places", request.ItemId, items[request.ItemId]?.Name, character.X, character.Y);
        return ValueTask.CompletedTask;
    }

    /// <summary>Boards the vehicle the player has already placed.</summary>
    /// <param name="session">The player's session.</param>
    /// <param name="body">The packet body after the action and sub code.</param>
    internal ValueTask OnBoard(GameSession session, ReadOnlyMemory<byte> body)
    {
        if (VehicleItemOf(session, body) is not { } request)
            return ValueTask.CompletedTask;
        if (session.Vehicle is not { } vehicle || vehicle.ItemId != request.ItemId)
        {
            logger.LogWarning("[{Session}] board {Item}: not placed by this player", session.Id, request.ItemId);
            return ValueTask.CompletedTask;
        }
        vehicle.Riding = true;
        var packet = VehicleMessages.Boarded(vehicle.BagSlot, session.Account!.LoginId, vehicle.ItemId);
        session.Send(packet);
        world.Broadcast(session, packet, sameMapOnly: true);
        if (session.ActiveCharacter!.Bag.Find(b => b.Slot == vehicle.BagSlot) is { } boarded)
            VehicleCareHandlers.SendState(session, boarded);
        logger.LogInformation("[{Session}] {Name} boards {Item}", session.Id, session.ActiveCharacter!.Name, vehicle.ItemId);
        return ValueTask.CompletedTask;
    }

    /// <summary>Gets the player off their vehicle and tells the map.</summary>
    /// <param name="session">The player's session.</param>
    /// <param name="body">The packet body after the action and sub code.</param>
    internal ValueTask OnPark(GameSession session, ReadOnlyMemory<byte> body)
    {
        if (VehicleItemOf(session, body) is not { } request)
            return ValueTask.CompletedTask;
        var bagSlot = session.Vehicle?.BagSlot ?? request.BagSlot;
        if (BreakRaftOnLanding(session, bagSlot, request.ItemId))
            return ValueTask.CompletedTask;
        session.Vehicle = null;
        var packet = VehicleMessages.Dismounted(bagSlot, session.Account!.LoginId);
        session.Send(packet);
        world.Broadcast(session, packet, sameMapOnly: true);
        logger.LogInformation("[{Session}] {Name} gets off {Item}", session.Id, session.ActiveCharacter!.Name, request.ItemId);
        return ValueTask.CompletedTask;
    }

    /// <summary>Item id of Robinson's Raft.</summary>
    public const ushort RobinsonsRaft = 48016; // Robinson's Raft

    /// <summary>Breaks Robinson's Raft when it lands at the starter's beach: the rider is put down, the wreck is shown and the raft leaves the bag.</summary>
    /// <param name="session">The player's session.</param>
    /// <param name="bagSlot">The bag slot of the vehicle.</param>
    /// <param name="itemId">The item id of the vehicle.</param>
    /// <returns>True when the raft broke.</returns>
    private bool BreakRaftOnLanding(GameSession session, byte bagSlot, ushort itemId)
    {
        if (itemId != RobinsonsRaft || session.ActiveCharacter is not { } character || character.MapId != WorldHandlers.StartersBeach.MapId)
            return false;
        var id = session.Account!.LoginId;
        session.Vehicle = null;
        var off = VehicleMessages.Dismounted(bagSlot, id);
        session.Send(off);
        world.Broadcast(session, off, sameMapOnly: true);
        var wreck = VehicleMessages.Destroyed(id, RobinsonsRaft);
        session.Send(wreck);
        world.Broadcast(session, wreck, sameMapOnly: true);
        if (character.Bag.Find(b => b.Slot == bagSlot && b.ItemId == RobinsonsRaft) is { } stack)
        {
            character.Bag.Remove(stack);
            session.Send(Protocol.Messages.Items.ItemMessages.BagSlotDecrease(stack.Slot, stack.Count));
        }
        logger.LogInformation("[{Session}] {Name} lands on the starter's beach: Robinson's Raft breaks", session.Id, character.Name);
        return true;
    }

    /// <summary>Reads a vehicle request and checks that the named bag slot holds that vehicle.</summary>
    /// <param name="session">The player's session.</param>
    /// <param name="body">The packet body after the action and sub code.</param>
    /// <returns>The request, or null when the player is not in the world or the slot does not hold that vehicle.</returns>
    private VehicleRequest? VehicleItemOf(GameSession session, ReadOnlyMemory<byte> body)
    {
        if (session.State != SessionState.InWorld || session.ActiveCharacter is null || body.Length < 3)
            return null;
        var request = VehicleRequest.Parse(body.Span);
        var slot = request.BagSlot;
        var item = session.ActiveCharacter.Bag.Find(b => b.Slot == slot);
        if (item is null || item.ItemId != request.ItemId || items[request.ItemId]?.EquipSlot != 7)
        {
            logger.LogWarning("[{Session}] vehicle request for {Item} in bag slot {Slot}: no such vehicle there", session.Id, request.ItemId, slot);
            return null;
        }
        return request;
    }
}
