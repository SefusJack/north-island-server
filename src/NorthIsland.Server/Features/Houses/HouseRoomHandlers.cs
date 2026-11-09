using Microsoft.Extensions.Logging;
using NorthIsland.GameData.Items;
using NorthIsland.Protocol.IO;
using NorthIsland.Protocol.Messages.Items;
using NorthIsland.Protocol.Messages.World;
using NorthIsland.Protocol.Opcodes;

namespace NorthIsland.Server.Features.Houses;

/// <summary>
/// The rooms of a house: the two floors with the Stairs or Elevator, and the rented rooms behind the Cherry, Beach and Sweet Doors.
/// </summary>
/// <param name="items">The item data, for names.</param>
/// <param name="world">The world state, to move players between maps.</param>
/// <param name="tents">The house and furniture handlers, for house owners and the house entry event.</param>
/// <param name="maps">The map data, to find the door of a rented room.</param>
/// <param name="logger">The logger.</param>
public sealed class HouseRoomHandlers(
    ItemData items,
    WorldState world,
    TentHandlers tents,
    MapData maps,
    ILogger<HouseRoomHandlers> logger)
{
    /// <summary>The item ids of the Stairs, the Elevator and the Cherry, Beach and Sweet Doors.</summary>
    public const ushort Stairs = 39031, // Stairs
        Elevator = 39014, // Elevator
        CherryDoor = 39107, // Cherry Door
        BeachDoor = 39108, // Beach Door
        SweetDoor = 39111; // Sweet Door

    /// <summary>The item ids of the vouchers that rent the Cherry and the Beach Door.</summary>
    public const ushort CherryVoucher = 34157, BeachVoucher = 34158; // 34157 Cherry Door Voucher, 34158 Beach Door Voucher

    /// <summary>How long a rented door lasts, in minutes (30 days).</summary>
    public const int RentMinutes = 43200;

    /// <summary>The room number a door leads to.</summary>
    /// <param name="itemId">The item id of the door.</param>
    /// <returns>2 for the Cherry Door, 3 for the Beach Door, 4 for the Sweet Door, 0 for any other item.</returns>
    public static byte RoomOfDoor(ushort itemId) => itemId switch
    {
        CherryDoor => 2, // Cherry Door leads to room 2
        BeachDoor => 3, // Beach Door leads to room 3
        SweetDoor => 4, // Sweet Door leads to room 4
        _ => 0 // any other item has no room
    };

    /// <summary>The map of a rented room.</summary>
    /// <param name="room">The room number, 2 to 4.</param>
    /// <returns>The map id, or 0 for any other room.</returns>
    private static ushort MapOfRoom(byte room) => room switch
    {
        2 => 10081, // Cherry Grassplot
        3 => 10083, // Summer Beach
        4 => 10084, // Happy House
        _ => 0 // any other room has no map
    };

    /// <summary>The number a door has in the rent time packet.</summary>
    /// <param name="itemId">The item id of the door.</param>
    /// <returns>1 for the Cherry Door, 2 for the Beach Door, 3 for the Sweet Door, 0 for any other item.</returns>
    private static byte RentIndex(ushort itemId) => itemId switch
    {
        CherryDoor => 1, // Cherry Door is 1
        BeachDoor => 2, // Beach Door is 2
        SweetDoor => 3, // Sweet Door is 3
        _ => 0 // any other item has no number
    };

    /// <summary>Hooks the house entry event so rents that ran out are removed.</summary>
    internal void Start()
    {
        tents.HouseEntered += OnHouseEntered;
    }

    /// <summary>
    /// Rents a door when the item used is a Cherry or Beach voucher: the door is delivered as a retained piece and the voucher is used up.
    /// </summary>
    /// <param name="session">The player's session.</param>
    /// <param name="c">The player's character.</param>
    /// <param name="stack">The bag stack that was used.</param>
    /// <param name="item">The record of the used item.</param>
    /// <returns>True when the item is a rent voucher, whether or not the rent started.</returns>
    public bool TryUseVoucher(GameSession session, Character c, BagItem stack, ItemRecord item)
    {
        var door = item.Id switch
        {
            CherryVoucher => CherryDoor, // Cherry voucher rents the Cherry Door
            BeachVoucher => BeachDoor, // Beach voucher rents the Beach Door
            _ => (ushort)0 // any other item is no rent voucher
        };
        if (door == 0)
            return false;
        if (c.Furniture.Exists(f => f.RentStartUtc is not null && RoomOfDoor(f.ItemId) is 2 or 3))
        {
            session.Send(HouseMessages.RentRefused(4)); // kind
            return true;
        }
        var index = TentHandlers.FreeIndex(c);
        if (index == 0)
        {
            session.Send(HouseMessages.RentRefused(3)); // kind
            return true;
        }
        c.Furniture.Add(new Furniture { Index = index, ItemId = door, Retained = true, RentStartUtc = GameClock.UtcNow });
        stack.Count--;
        if (stack.Count == 0)
            c.Bag.Remove(stack);
        session.Send(ItemMessages.BagSlotDecrease(stack.Slot, 1));
        session.Send(HouseMessages.FurnitureDelivered(index, door));
        session.Send(HouseMessages.RentNotice(1, door));
        logger.LogInformation("[{Session}] {Name} rents {Door} for 30 days (furniture {Index})", session.Id, c.Name, items[door]?.Name.Trim(), index);
        return true;
    }

    /// <summary>Removes the rented doors whose time is over and tells the owner if they are online.</summary>
    /// <param name="c">The character who rents.</param>
    /// <param name="owner">The owner's session, or null when they are not online.</param>
    public void Expire(Character c, GameSession? owner)
    {
        var now = GameClock.UtcNow;
        foreach (var door in c.Furniture.Where(f => f.RentStartUtc is { } start && (now - start).TotalMinutes >= RentMinutes).ToList())
        {
            c.Furniture.Remove(door);
            if (owner is null)
                continue;
            owner.Send(HouseMessages.RentNotice(2, door.ItemId)); // kind
            if (!door.Retained)
                owner.Send(HouseMessages.FurnitureRemoved(door.Index));
            else
                owner.Send(HouseMessages.FurnitureRetained(door.Index, false));
            logger.LogInformation("[{Session}] {Name}: the rent of {Door} is over", owner.Id, c.Name, items[door.ItemId]?.Name.Trim());
        }
    }

    /// <summary>At login removes expired rents and sends the time used on each rented door.</summary>
    /// <param name="session">The player's session.</param>
    /// <param name="c">The player's character.</param>
    public void SendOnLogin(GameSession session, Character c)
    {
        Expire(c, session);
        foreach (var door in c.Furniture.Where(f => f.RentStartUtc is not null))
            session.Send(HouseMessages.RentTime(RentIndex(door.ItemId), (int)(GameClock.UtcNow - door.RentStartUtc!.Value).TotalMinutes));
    }

    /// <summary>Removes expired rents of the house owner when someone enters the house.</summary>
    /// <param name="session">The session that entered the house.</param>
    /// <param name="owner">The house owner's session, or null when the owner is not online.</param>
    private void OnHouseEntered(GameSession session, GameSession? owner)
    {
        if (owner?.ActiveCharacter is { } c)
            Expire(c, owner);
    }

    /// <summary>
    /// Handles <c>62/14</c>: a click on the Stairs, the Elevator or a rented door moves the player to the other floor or the rented room.
    /// </summary>
    /// <param name="session">The player's session.</param>
    /// <param name="body">The furniture index (two bytes).</param>
    internal ValueTask OnClick(GameSession session, ReadOnlyMemory<byte> body)
    {
        var c = session.ActiveCharacter;
        if (session.State != SessionState.InWorld || c is null || session.Battle is not null || body.Length < 2 || !TentHandlers.IsHouseMap(c.MapId))
            return ValueTask.CompletedTask;
        var host = tents.OwnerOfHouse(c.MapId)?.Character;
        var index = new PacketReader(body.Span).U16();
        if (host?.Furniture.Find(f => f.Index == index && !f.Retained) is not { } piece)
            return ValueTask.CompletedTask;
        byte target;
        if (piece.ItemId is Stairs or Elevator)
        {
            if (session.HouseRoom > 1 || piece.Room != session.HouseRoom)
            {
                if (piece.ItemId == Elevator)
                    session.Send(new PacketWriter(Action62.Code, Action62.LiftStop).U8(1).U16(piece.Index).ToArray());
                return ValueTask.CompletedTask;
            }
            target = (byte)(1 - session.HouseRoom);
        }
        else if (RoomOfDoor(piece.ItemId) is var room and > 1)
        {
            if (session.HouseRoom > 1 || (piece.RentStartUtc is { } start && (GameClock.UtcNow - start).TotalMinutes >= RentMinutes))
                return ValueTask.CompletedTask;
            target = room;
        }
        else
            return ValueTask.CompletedTask;
        var (x, y) = target > 1 ? ArrivalOf(target) : (TentHandlers.SpawnX, TentHandlers.SpawnY);
        logger.LogInformation("[{Session}] {Name} takes {Piece} into room {Room}", session.Id, c.Name, items[piece.ItemId]?.Name.Trim(), target);
        world.ChangeMap(session, new DoorWarp(c.MapId, x, y, 0, target > 1 ? (byte)0 : tents.RoomSizeOf(c.MapId), target));
        return ValueTask.CompletedTask;
    }

    /// <summary>Where a player arrives in a rented room.</summary>
    /// <param name="room">The room number, 2 to 4.</param>
    /// <returns>The arrival position, just above the room's door, or the house's spawn point when the map has no door.</returns>
    private (ushort X, ushort Y) ArrivalOf(byte room)
    {
        if (maps.Scene(MapOfRoom(room))?.DoorByIndex(1) is { } door)
        {
            var area = door.Area;
            return ((ushort)Math.Clamp(area.Left + area.Width / 2, 0, ushort.MaxValue), (ushort)Math.Clamp(area.Top - 40, 0, ushort.MaxValue));
        }
        return (TentHandlers.SpawnX, TentHandlers.SpawnY);
    }
}
