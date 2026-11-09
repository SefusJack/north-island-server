using NorthIsland.Protocol.IO;
using NorthIsland.Protocol.Opcodes;

namespace NorthIsland.Protocol.Messages.World;

/// <summary>Builders for house, tent and furniture packets (actions 62, 64 and 65, and the tent style chooser in action 23).</summary>
public static class HouseMessages
{
    /// <summary>Size in bytes of one furniture record on the wire.</summary>
    public const int FurnitureRecordSize = 57;

    /// <summary>Builds the <c>65</c> packet that shows one newly pitched tent.</summary>
    /// <param name="camp">The tent.</param>
    /// <returns>The packet.</returns>
    public static byte[] CampAdded(CampRecord camp) => WriteCamp(new PacketWriter(Action65.Code, Action65.Added), camp).ToArray();

    /// <summary>Builds the <c>65</c> packet with all pitched tents.</summary>
    /// <param name="camps">The tents.</param>
    /// <returns>The packet.</returns>
    public static byte[] CampList(IEnumerable<CampRecord> camps)
    {
        var w = new PacketWriter(Action65.Code, Action65.List);
        foreach (var camp in camps)
            WriteCamp(w, camp);
        return w.ToArray();
    }

    /// <summary>Builds the <c>65</c> packet that removes the tent of a player.</summary>
    /// <param name="ownerId">Id of the tent owner.</param>
    /// <returns>The packet.</returns>
    public static byte[] CampTakenBack(int ownerId) => new PacketWriter(Action65.Code, Action65.TakenBack).I32(ownerId).ToArray();

    /// <summary>Builds the <c>65</c> packet that tells a visitor the host has left.</summary>
    /// <returns>The packet.</returns>
    public static byte[] HostLeft() => new PacketWriter(Action65.Code, Action65.HostLeft).ToArray();

    /// <summary>Builds the <c>65</c> packet that sets whether the house is locked.</summary>
    /// <param name="locked">True when the house is locked.</param>
    /// <param name="showMessage">True to show a message about the change.</param>
    /// <returns>The packet.</returns>
    public static byte[] HouseLock(bool locked, bool showMessage) =>
        new PacketWriter(Action65.Code, Action65.Lock).Bool(locked).Bool(showMessage).ToArray();

    /// <summary>Builds the <c>65</c> packet that changes the style of a tent.</summary>
    /// <param name="ownerId">Id of the tent owner.</param>
    /// <param name="kind1">First style byte.</param>
    /// <param name="kind2">Second style byte.</param>
    /// <returns>The packet.</returns>
    public static byte[] StyleChanged(int ownerId, byte kind1, byte kind2) =>
        new PacketWriter(Action65.Code, Action65.StyleChanged).I32(ownerId).U8(kind1).U8(kind2).ToArray();

    /// <summary>Builds the packet that opens the tent style chooser.</summary>
    /// <param name="bagSlot">Bag slot of the tent item.</param>
    /// <param name="kindSet">Which set of styles to offer.</param>
    /// <returns>The packet.</returns>
    public static byte[] StyleChooser(byte bagSlot, byte kindSet) =>
        new PacketWriter(Action23.Code, Action23.TentStyleChooser).U8(bagSlot).U8(kindSet).ToArray();

    /// <summary>Builds the packet that tells the player a style change was refused.</summary>
    /// <param name="code">Reason code.</param>
    /// <returns>The packet.</returns>
    public static byte[] StyleRefused(byte code) => new PacketWriter(Action23.Code, Action23.TentStyleRefused).U8(code).ToArray();

    /// <summary>Builds the <c>65</c> packet that tells the player they may not enter the house.</summary>
    /// <returns>The packet.</returns>
    public static byte[] EnterForbidden() => new PacketWriter(Action65.Code, Action65.Forbidden).ToArray();

    /// <summary>Builds the <c>62</c> packet that adds a piece of furniture to the house.</summary>
    /// <param name="index">Furniture index.</param>
    /// <param name="record">The furniture record.</param>
    /// <returns>The packet.</returns>
    public static byte[] FurnitureAdded(ushort index, FurnitureRecord record) =>
        WriteFurniture(new PacketWriter(Action62.Code, Action62.Place).U16(index), record).ToArray();

    /// <summary>Builds the <c>62</c> packet that removes a piece of furniture.</summary>
    /// <param name="index">Furniture index.</param>
    /// <returns>The packet.</returns>
    public static byte[] FurnitureRemoved(ushort index) => new PacketWriter(Action62.Code, Action62.TakeBack).U16(index).ToArray();

    /// <summary>Builds the <c>65/12</c> packet that sets the size of the room of a floor.</summary>
    /// <param name="size">Room size, 1 (small) to 3.</param>
    /// <param name="floor">The floor the room belongs to.</param>
    /// <returns>The packet.</returns>
    public static byte[] RoomSize(byte size, byte floor) => new PacketWriter(Action65.Code, Action65.Room).U8(size).U8(floor).ToArray();

    /// <summary>Builds the <c>62</c> packet that sets the retained flag of a piece of furniture.</summary>
    /// <param name="index">Furniture index.</param>
    /// <param name="retained">The new flag value.</param>
    /// <returns>The packet.</returns>
    public static byte[] FurnitureRetained(ushort index, bool retained) =>
        new PacketWriter(Action62.Code, Action62.Retain).U16(index).Bool(retained).ToArray();

    /// <summary>Builds the <c>62</c> packet that confirms the placement of a piece of furniture.</summary>
    /// <param name="index">Furniture index.</param>
    /// <param name="x">Cell x.</param>
    /// <param name="y">Cell y.</param>
    /// <param name="z">Level.</param>
    /// <param name="dir">Facing direction.</param>
    /// <param name="room">Room number.</param>
    /// <returns>The packet.</returns>
    public static byte[] FurniturePlaced(
        ushort index,
        int x,
        int y,
        int z,
        byte dir,
        byte room) =>
            new PacketWriter(Action62.Code, Action62.Placed).U16(index).I32(x).I32(y).I32(z).U8(dir).U8(room).ToArray();

    /// <summary>Builds the <c>62</c> packet that reports a delivered piece of furniture.</summary>
    /// <param name="index">Furniture index.</param>
    /// <param name="itemId">Item id of the furniture.</param>
    /// <returns>The packet.</returns>
    public static byte[] FurnitureDelivered(ushort index, ushort itemId) =>
        new PacketWriter(Action62.Code, Action62.Delivered).U16(index).U8(1).U16(itemId).U8(1).ToArray();

    /// <summary>Builds the <c>62</c> rent packet with an item id.</summary>
    /// <param name="kind">Notice kind.</param>
    /// <param name="itemId">Item id the notice is about.</param>
    /// <returns>The packet.</returns>
    public static byte[] RentNotice(byte kind, ushort itemId) => new PacketWriter(Action62.Code, Action62.RentNotice).U8(kind).U16(itemId).ToArray();

    /// <summary>Builds the <c>62</c> rent packet that carries only a kind byte.</summary>
    /// <param name="kind">Notice kind.</param>
    /// <returns>The packet.</returns>
    public static byte[] RentRefused(byte kind) => new PacketWriter(Action62.Code, Action62.RentNotice).U8(kind).ToArray();

    /// <summary>Builds the <c>62</c> rent packet (kind 5) with the time used.</summary>
    /// <param name="rent">Rent byte.</param>
    /// <param name="minutesUsed">Minutes already used.</param>
    /// <returns>The packet.</returns>
    public static byte[] RentTime(byte rent, int minutesUsed) =>
        new PacketWriter(Action62.Code, Action62.RentNotice).U8(5).U8(rent).I32(minutesUsed).ToArray();

    /// <summary>Builds the <c>62</c> packet that moves a piece of furniture.</summary>
    /// <param name="index">Furniture index.</param>
    /// <param name="x">Cell x.</param>
    /// <param name="y">Cell y.</param>
    /// <param name="z">Level.</param>
    /// <param name="dir">Facing direction.</param>
    /// <returns>The packet.</returns>
    public static byte[] FurnitureMoved(
        ushort index,
        int x,
        int y,
        int z,
        byte dir) =>
            new PacketWriter(Action62.Code, Action62.Move).U16(index).I32(x).I32(y).I32(z).U8(dir).ToArray();

    /// <summary>Builds the <c>62/4</c> packet with the owner id and all furniture of the house.</summary>
    /// <param name="houseOwnerId">Id of the house owner.</param>
    /// <param name="furniture">Furniture index and record of each piece.</param>
    /// <returns>The packet.</returns>
    public static byte[] FurnitureList(int houseOwnerId, IEnumerable<(ushort Index, FurnitureRecord Record)> furniture)
    {
        var w = new PacketWriter(Action62.Code, Action62.List).I32(houseOwnerId);
        foreach (var (index, record) in furniture)
            WriteFurniture(w.U16(index), record);
        return w.ToArray();
    }

    /// <summary>Builds the <c>62</c> packet that attaches a tool to a work platform.</summary>
    /// <param name="index">Furniture index of the tool.</param>
    /// <param name="itemId">Item id of the tool.</param>
    /// <param name="parent">Furniture index of the platform.</param>
    /// <returns>The packet.</returns>
    public static byte[] ToolLinked(ushort index, ushort itemId, ushort parent) =>
        new PacketWriter(Action62.Code, Action62.ToolLinked).U16(index).U16(itemId).U16(parent).ToArray();

    /// <summary>Builds the <c>64</c> packet with the running manufacturing jobs.</summary>
    /// <param name="jobs">The jobs.</param>
    /// <returns>The packet.</returns>
    public static byte[] MakeJobs(IEnumerable<MakeJobRecord> jobs)
    {
        var w = new PacketWriter(Action64.Code, Action64.Jobs);
        foreach (var j in jobs)
            w.U8(j.Slot).U16(j.Product).I32(j.ElapsedMs).U8(j.Batches);
        return w.ToArray();
    }

    /// <summary>Builds a <c>64</c> packet that names a job slot.</summary>
    /// <param name="sub">The sub code to send.</param>
    /// <param name="slot">The job slot.</param>
    /// <returns>The packet.</returns>
    public static byte[] MakeState(byte sub, byte slot) => new PacketWriter(Action64.Code, sub).U8(slot).ToArray();

    /// <summary>Builds the <c>64</c> packet with the fuel state of a gas tank.</summary>
    /// <param name="tank">Tank number.</param>
    /// <param name="fuel">Fuel amount.</param>
    /// <param name="finished">True when the fuel is used up.</param>
    /// <returns>The packet.</returns>
    public static byte[] GasState(byte tank, ushort fuel, bool finished) =>
        new PacketWriter(Action64.Code, Action64.GasState).U16(fuel).Bool(finished).U8(tank).ToArray();

    /// <summary>Builds the <c>64</c> packet that refuses a gas action.</summary>
    /// <param name="tank">Tank number; 1 sends the large tank packet, anything else the small one.</param>
    /// <returns>The packet.</returns>
    public static byte[] GasRefused(byte tank) => new PacketWriter(Action64.Code, tank == 1 ? Action64.GasRefused : Action64.SmallGasRefused).ToArray();

    /// <summary>Builds the <c>64</c> packet that tells the client the pet has left the pet maker.</summary>
    /// <returns>The packet.</returns>
    public static byte[] PetLeft() => new PacketWriter(Action64.Code, Action64.PetLeft).ToArray();

    /// <summary>Builds the <c>64</c> packet that refuses a pet for the pet maker.</summary>
    /// <returns>The packet.</returns>
    public static byte[] PetRefused() => new PacketWriter(Action64.Code, Action64.PetRefused).ToArray();

    /// <summary>Builds the <c>64</c> packet that puts a pet into the pet maker.</summary>
    /// <param name="petSlot">Slot of the pet.</param>
    /// <param name="petId">Id of the pet.</param>
    /// <returns>The packet.</returns>
    public static byte[] PetMaker(byte petSlot, int petId) => new PacketWriter(Action64.Code, Action64.PetMaker).U8(petSlot).I32(petId).ToArray();

    /// <summary>Writes a tent record.</summary>
    /// <param name="w">The writer to append to.</param>
    /// <param name="c">The tent.</param>
    /// <returns>The same writer.</returns>
    private static PacketWriter WriteCamp(PacketWriter w, CampRecord c) =>
        w.I32(c.OwnerId).U16(c.ItemId).I32(c.X).I32(c.Y).U8(c.Kind1).U8(c.Kind2);

    /// <summary>Writes a furniture record as a 57-byte block.</summary>
    /// <param name="w">The writer to append to.</param>
    /// <param name="r">The furniture record.</param>
    /// <returns>The same writer.</returns>
    private static PacketWriter WriteFurniture(PacketWriter w, FurnitureRecord r)
    {
        Span<byte> rec = stackalloc byte[FurnitureRecordSize];
        rec.Clear();
        var inner = new PacketWriter().U16(r.ItemId).I32(r.X).I32(r.Y).I32(r.Z).U8(r.Dir).U16(r.Parent).U16(0);
        for (var i = 0; i < 10; i++)
            inner.U16(i < r.Children.Count ? r.Children[i] : (ushort)0);
        inner.ToArray().CopyTo(rec);
        rec[47] = r.Room;
        rec[48] = r.Retained ? (byte)1 : (byte)0;
        return w.Bytes(rec);
    }
}
