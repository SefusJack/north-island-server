using System.Buffers.Binary;
using Microsoft.Extensions.Logging;
using NorthIsland.Protocol.IO;
using NorthIsland.Protocol.Messages.Items;
using NorthIsland.Protocol.Messages.World;
using NorthIsland.Protocol.Opcodes;

namespace NorthIsland.Server.Features.Vehicles;

/// <summary>
/// Handles the Garage (a furniture piece with four cells for vehicles) and the Show Platform that displays one of them,
/// in the player's own house.
/// </summary>
/// <param name="items">The item records.</param>
/// <param name="world">The online sessions, used to tell everyone on the map what stands on a platform.</param>
/// <param name="tents">The house handler; its entry event is subscribed to and it finds the owner of a house.</param>
/// <param name="space">The Space Contract check for the garage part.</param>
/// <param name="logger">Logger for the handler's messages.</param>
public sealed class GarageHandlers(
    ItemData items,
    WorldState world,
    TentHandlers tents,
    SpaceContract space,
    ILogger<GarageHandlers> logger)
{
    /// <summary>Item ids of the Garage and of the Show Platform.</summary>
    public const ushort Garage = 39069, ShowPlatform = 39001; // 39069 Garage, 39001 Show Platform

    /// <summary>The number of vehicle cells in a garage.</summary>
    public const int Cells = 4;

    /// <summary>Subscribes to the event raised when a player enters a house.</summary>
    internal void Start()
    {
        tents.HouseEntered += OnHouseEntered;
    }

    /// <summary>Gives the active character when they are in the world and standing in their own house.</summary>
    /// <param name="session">The player's session.</param>
    /// <returns>The character, or null when the player is not in their own house.</returns>
    private static Character? InOwnHouse(GameSession session)
    {
        var character = session.ActiveCharacter;
        return session.State == SessionState.InWorld && character is not null && session.Account is not null
            && TentHandlers.HouseMapOf(session.Account.LoginId) == character.MapId ? character : null;
    }

    /// <summary>Lists every vehicle stored in the character's garages.</summary>
    /// <param name="c">The player's character.</param>
    /// <returns>The stored vehicles.</returns>
    private static IEnumerable<StockItem> Cars(Character c) => c.Furniture.Where(f => f.ItemId == Garage).SelectMany(f => f.Stock);

    /// <summary>Tells whether the character is restrained, that is, lacks the garage part of the Space Contract (cells 3 and 4 stay locked).</summary>
    /// <param name="c">The player's character.</param>
    /// <returns>True when the character is restrained.</returns>
    private bool Restrained(Character c) => !space.Has(c, SpaceContract.Garage);

    /// <summary>Builds a garage packet that carries the restrained flag.</summary>
    /// <param name="sub">The packet's sub code.</param>
    /// <param name="c">The player's character.</param>
    /// <returns>The packet.</returns>
    private byte[] Restrain(byte sub, Character c) => new PacketWriter(Action62.Code, sub).U8(Restrained(c) ? (byte)1 : (byte)0).ToArray();

    /// <summary>Builds the packet that shows a stored vehicle in its garage cell.</summary>
    /// <param name="f">The garage piece.</param>
    /// <param name="s">The stored vehicle.</param>
    /// <returns>The packet.</returns>
    private static byte[] CarPacket(Furniture f, StockItem s) =>
        new PacketWriter(Action62.Code, Action62.GaragePut).U16(f.Index).U8(s.Pos).U8(s.Id)
            .Bytes(HouseLifeMessages.CarItem(s.ItemId, 1, s.Wear, s.Insert, s.Fusion, s.Pose, s.BoxItem, s.Used, s.Extra)).ToArray();

    /// <summary>Builds the packet that tells which vehicle stands on a show platform.</summary>
    /// <param name="owner">The login id of the house owner.</param>
    /// <param name="cell">The garage cell of the vehicle, 0 when it is not told.</param>
    /// <param name="platform">The show platform piece.</param>
    /// <param name="itemId">The item id of the vehicle, 0 for none.</param>
    /// <returns>The packet.</returns>
    private static byte[] ExhibitPacket(int owner, int cell, Furniture platform, ushort itemId) =>
        new PacketWriter(Action62.Code, Action62.PlatformShown).I32(owner).I32(cell).U16(platform.Index).U16(itemId).ToArray();

    /// <summary>Answers a request to open a garage with the restrained flag, when the player has such a garage.</summary>
    /// <param name="session">The player's session.</param>
    /// <param name="body">The furniture index of the garage (u16).</param>
    internal ValueTask OnOpenGarage(GameSession session, ReadOnlyMemory<byte> body)
    {
        if (InOwnHouse(session) is not { } c || body.Length < 2)
            return ValueTask.CompletedTask;
        if (c.Furniture.Exists(f => f.Index == BinaryPrimitives.ReadUInt16LittleEndian(body.Span) && f.ItemId == Garage))
            session.Send(Restrain(Action62.GarageOpen, c));
        return ValueTask.CompletedTask;
    }

    /// <summary>Moves a vehicle from the bag into a free garage cell; refuses other items, locked or taken cells and the vehicle that is placed.</summary>
    /// <param name="session">The player's session.</param>
    /// <param name="body">The furniture index of the garage (u16), the bag slot and the cell.</param>
    internal ValueTask OnPut(GameSession session, ReadOnlyMemory<byte> body)
    {
        if (InOwnHouse(session) is not { } c || body.Length < 4)
            return ValueTask.CompletedTask;
        var r = new PacketReader(body.Span);
        var (furn, bagSlot, cell) = (r.U16(), r.U8(), r.U8());
        var garage = c.Furniture.Find(f => f.Index == furn && f.ItemId == Garage);
        var stack = c.Bag.Find(b => b.Slot == bagSlot);
        if (garage is null
            || stack is null
            || !ParkingHandlers.IsVehicle(stack.ItemId)
            || cell is < 1 or > Cells
            || (cell > 2 && Restrained(c))
            || garage.Stock.Exists(s => s.Pos == cell)
            || session.Vehicle?.BagSlot == bagSlot)
            return ValueTask.CompletedTask;
        var id = Enumerable.Range(1, Cells).Select(i => (byte)i).FirstOrDefault(i => !Cars(c).Any(s => s.Id == i));
        if (id == 0)
            return ValueTask.CompletedTask;
        var car = new StockItem
        {
            Id = id,
            Pos = cell,
            ItemId = stack.ItemId,
            Count = 1,
            Wear = stack.Wear,
            Insert = stack.Insert,
            Fusion = stack.Fusion,
            Pose = stack.Pose,
            BoxItem = stack.BoxItem,
            Used = stack.Used,
            Extra = stack.Extra
        };
        garage.Stock.Add(car);
        c.Bag.Remove(stack);
        session.Send(ItemMessages.BagSlotDecrease(stack.Slot, 1));
        session.Send(CarPacket(garage, car));
        logger.LogInformation("[{Session}] {Name} parks {Item} in garage {Furn} cell {Cell}", session.Id, c.Name, items[car.ItemId]?.Name.Trim(), furn, cell);
        return ValueTask.CompletedTask;
    }

    /// <summary>Moves the vehicle in a garage cell back into the bag, unless it stands on a show platform or the bag has no room.</summary>
    /// <param name="session">The player's session.</param>
    /// <param name="body">The furniture index of the garage (u16) and the cell.</param>
    internal ValueTask OnTake(GameSession session, ReadOnlyMemory<byte> body)
    {
        if (InOwnHouse(session) is not { } c || body.Length < 3)
            return ValueTask.CompletedTask;
        var r = new PacketReader(body.Span);
        var (furn, cell) = (r.U16(), r.U8());
        var garage = c.Furniture.Find(f => f.Index == furn && f.ItemId == Garage);
        if (garage?.Stock.Find(s => s.Pos == cell) is not { } car || c.Furniture.Exists(f => f.ItemId == ShowPlatform && f.Exhibit == car.Id))
            return ValueTask.CompletedTask;
        var slot = Bag.FreeSlot(c, items, car.ItemId);
        if (slot == 0)
            return ValueTask.CompletedTask;
        c.Bag.Add(
            new BagItem
            {
                Slot = slot,
                ItemId = car.ItemId,
                Count = 1,
                Wear = car.Wear,
                Insert = car.Insert,
                Fusion = car.Fusion,
                Pose = car.Pose,
                BoxItem = car.BoxItem,
                Used = car.Used,
                Extra = car.Extra
            });
        session.Send(
            ItemMessages.BagAdd(
                new BagEntry
                {
                    Slot = slot,
                    ItemId = car.ItemId,
                    Count = 1,
                    Wear = car.Wear,
                    Insert = car.Insert,
                    FusionLevel = car.Fusion,
                    PoseItemId = car.Pose,
                    BoxItemId = car.BoxItem,
                    UsedAmount = car.Used,
                    ExtraAmount = car.Extra
                }));
        garage.Stock.Remove(car);
        session.Send(new PacketWriter(Action62.Code, Action62.GarageTake).U16(furn).U8(cell).U8(car.Id).ToArray());
        return ValueTask.CompletedTask;
    }

    /// <summary>Answers a request to open a show platform with the restrained flag, when the player has such a platform.</summary>
    /// <param name="session">The player's session.</param>
    /// <param name="body">The furniture index of the platform (u16).</param>
    internal ValueTask OnOpenPlatform(GameSession session, ReadOnlyMemory<byte> body)
    {
        if (InOwnHouse(session) is not { } c || body.Length < 2)
            return ValueTask.CompletedTask;
        if (c.Furniture.Exists(f => f.Index == BinaryPrimitives.ReadUInt16LittleEndian(body.Span) && f.ItemId == ShowPlatform))
            session.Send(Restrain(Action62.PlatformForm, c));
        return ValueTask.CompletedTask;
    }

    /// <summary>Puts a garage vehicle on a show platform or takes it down, and tells everyone on the map.</summary>
    /// <param name="session">The player's session.</param>
    /// <param name="body">Ten bytes or more take the vehicle in a cell (u16) down; otherwise a platform index (u16) and a cell (i32) put it up.</param>
    internal ValueTask OnSet(GameSession session, ReadOnlyMemory<byte> body)
    {
        if (InOwnHouse(session) is not { } c || body.Length < 6)
            return ValueTask.CompletedTask;
        var first = BinaryPrimitives.ReadUInt16LittleEndian(body.Span);
        var second = BinaryPrimitives.ReadInt32LittleEndian(body.Span[2..]);
        Furniture? platform;
        StockItem? car;
        if (body.Length >= 10)
        {
            platform = c.Furniture.Find(f => f.ItemId == ShowPlatform && f.Exhibit != 0 && Cars(c).Any(s => s.Id == f.Exhibit && s.Pos == first));
            car = null;
        }
        else
        {
            platform = c.Furniture.Find(f => f.Index == first && f.ItemId == ShowPlatform);
            car = Cars(c).FirstOrDefault(s => s.Pos == second);
            if (car is null || c.Furniture.Exists(f => f.ItemId == ShowPlatform && f.Exhibit == car.Id))
                return ValueTask.CompletedTask;
        }
        if (platform is null)
            return ValueTask.CompletedTask;
        platform.Exhibit = car?.Id ?? 0;
        var packet = ExhibitPacket(session.Account!.LoginId, car?.Pos ?? 0, platform, car?.ItemId ?? 0);
        foreach (var s in world.OnMap(c.MapId).Append(session).Distinct())
            s.Send(packet);
        return ValueTask.CompletedTask;
    }

    /// <summary>Sends a player who entered a house the vehicles on show there (and, for the owner, the garage contents).</summary>
    /// <param name="session">The session of the player who entered.</param>
    /// <param name="owner">The session of the house owner when they are online, otherwise null.</param>
    private void OnHouseEntered(GameSession session, GameSession? owner)
    {
        var host = owner?.ActiveCharacter ?? tents.OwnerOfHouse(session.ActiveCharacter?.MapId ?? 0)?.Character;
        var hostId = owner?.Account?.LoginId ?? tents.OwnerOfHouse(session.ActiveCharacter?.MapId ?? 0)?.Id ?? 0;
        if (host is null)
            return;
        if (owner == session)
            foreach (var garage in host.Furniture.Where(f => f.ItemId == Garage))
                foreach (var car in garage.Stock)
                    session.Send(CarPacket(garage, car));
        foreach (var platform in host.Furniture.Where(f => f.ItemId == ShowPlatform && f.Exhibit != 0))
            if (Cars(host).FirstOrDefault(s => s.Id == platform.Exhibit) is { } shown)
                session.Send(ExhibitPacket(hostId, owner == session ? shown.Pos : 0, platform, shown.ItemId));
    }
}
