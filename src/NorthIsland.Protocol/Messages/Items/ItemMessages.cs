using NorthIsland.Protocol.IO;
using NorthIsland.Protocol.Opcodes;

namespace NorthIsland.Protocol.Messages.Items;

/// <summary>Builders for the bag and equipment packets (actions 5 and 23).</summary>
public static class ItemMessages
{
    /// <summary>Builds 23/5, the whole bag.</summary>
    /// <param name="entries">The stacks in the bag.</param>
    /// <returns>The packet.</returns>
    public static byte[] BagList(IEnumerable<BagEntry> entries)
    {
        var w = new PacketWriter(Action23.Code, Action23.BagList);
        foreach (var e in entries)
            e.WriteTo(w);
        return w.ToArray();
    }

    /// <summary>Builds 23/11, the worn equipment.</summary>
    /// <param name="entries">The worn items.</param>
    /// <returns>The packet.</returns>
    public static byte[] EquipList(IEnumerable<EquipEntry> entries)
    {
        var w = new PacketWriter(Action23.Code, Action23.EquipList);
        foreach (var e in entries)
            e.WriteTo(w);
        return w.ToArray();
    }

    /// <summary>Builds 23/8, which adds items to a bag slot.</summary>
    /// <param name="entry">The stack to add; its slot says where.</param>
    /// <returns>The packet.</returns>
    public static byte[] BagAdd(BagEntry entry)
    {
        var w = new PacketWriter(Action23.Code, Action23.BagAdd);
        entry.WriteTo(w);
        return w.ToArray();
    }

    /// <summary>Builds the packet that reports the result of a compound.</summary>
    /// <param name="itemId">Id of the resulting item.</param>
    /// <param name="slot">Slot of the result.</param>
    /// <returns>The packet.</returns>
    public static byte[] CompoundResult(ushort itemId, byte slot) =>
        new PacketWriter(Action23.Code, Action23.CompoundResult).U16(itemId).U8(0).U8(slot).ToArray();

    /// <summary>Builds 23/77, which reports a magic crystal stone set in a worn item.</summary>
    /// <param name="slot">Equipment slot of the item.</param>
    /// <param name="stoneId">Id of the stone.</param>
    /// <returns>The packet.</returns>
    public static byte[] CrystalMounted(byte slot, ushort stoneId) =>
        new PacketWriter(Action23.Code, Action23.CrystalMounted).U8(slot).U16(stoneId).ToArray();

    /// <summary>Builds the packet that sets a player's colour code.</summary>
    /// <param name="second">True for the second code (clothes, eyes), false for the first (hair, skin).</param>
    /// <param name="playerId">Id of the player.</param>
    /// <param name="code">The colour code.</param>
    /// <returns>The packet.</returns>
    public static byte[] ColourCode(bool second, int playerId, int code) =>
        new PacketWriter(Action23.Code, second ? Action23.ColourCode2 : Action23.ColourCode1).I32(playerId).I32(code).ToArray();

    /// <summary>Builds the packet that sets the wear value of a worn item.</summary>
    /// <param name="part">Equipment part.</param>
    /// <param name="wear">The new wear value.</param>
    /// <returns>The packet.</returns>
    public static byte[] EquipWear(byte part, byte wear) => new PacketWriter(Action23.Code, Action23.EquipWear).U8(part).U8(wear).ToArray();

    /// <summary>Builds the packet that reports a worn item broke.</summary>
    /// <param name="owner">0 for the player, otherwise the slot of the pet that wore it.</param>
    /// <param name="itemId">Id of the broken item.</param>
    /// <returns>The packet.</returns>
    public static byte[] EquipBroken(byte owner, ushort itemId) => new PacketWriter(Action23.Code, Action23.EquipBroken).U8(owner).U16(itemId).ToArray();

    /// <summary>Builds the packet that sets the wear value of an item worn by a pet.</summary>
    /// <param name="petSlot">Slot of the pet.</param>
    /// <param name="part">Equipment part.</param>
    /// <param name="wear">The new wear value.</param>
    /// <returns>The packet.</returns>
    public static byte[] PetEquipWear(byte petSlot, byte part, byte wear) =>
        new PacketWriter(Action23.Code, Action23.PetEquipWear).U8(petSlot).U8(part).U8(wear).ToArray();

    /// <summary>Builds an action 5 packet that changes the shape of players.</summary>
    /// <param name="shapes">Player id and NPC id whose look the player takes.</param>
    /// <returns>The packet.</returns>
    public static byte[] Transform(IEnumerable<(int PlayerId, ushort NpcId)> shapes)
    {
        var w = new PacketWriter(Action5.Code, Action5.Transform);
        foreach (var (id, npc) in shapes)
            w.I32(id).U16(npc);
        return w.ToArray();
    }

    /// <summary>Builds the body shift notice that reports its state.</summary>
    /// <param name="state">The state byte.</param>
    /// <returns>The packet.</returns>
    public static byte[] BodyShiftState(byte state) => new PacketWriter(Action23.Code, Action23.BodyShiftNotice).U8(1).U8(state).ToArray();

    /// <summary>Builds the body shift notice that asks the player to confirm.</summary>
    /// <param name="minutes">Number of minutes shown in the question.</param>
    /// <returns>The packet.</returns>
    public static byte[] BodyShiftAsk(ushort minutes) => new PacketWriter(Action23.Code, Action23.BodyShiftNotice).U8(2).U16(minutes).ToArray();

    /// <summary>Builds the body shift notice that reports a refusal.</summary>
    /// <param name="reason">Reason byte.</param>
    /// <returns>The packet.</returns>
    public static byte[] BodyShiftRefused(byte reason) => new PacketWriter(Action23.Code, Action23.BodyShiftNotice).U8(3).U8(reason).ToArray();

    /// <summary>Builds the experience pause notice that reports the current state.</summary>
    /// <param name="paused">True when experience gain is paused.</param>
    /// <returns>The packet.</returns>
    public static byte[] ExpPaused(bool paused) => new PacketWriter(Action23.Code, Action23.ExpPauseNotice).U8(1).Bool(paused).ToArray();

    /// <summary>Builds the experience pause notice that reports the use of the pause item.</summary>
    /// <param name="paused">True when experience gain is paused now.</param>
    /// <returns>The packet.</returns>
    public static byte[] ExpPauseUsed(bool paused) => new PacketWriter(Action23.Code, Action23.ExpPauseNotice).U8(2).Bool(paused).ToArray();

    /// <summary>Builds the packet that sets the wear value of a bag slot.</summary>
    /// <param name="slot">Bag slot.</param>
    /// <param name="wear">The new wear value.</param>
    /// <returns>The packet.</returns>
    public static byte[] BagWear(byte slot, int wear) =>
        new PacketWriter(Action23.Code, Action23.WearGroup).U8(1).U8(1).U8(slot).I32(wear).U8(0).ToArray();

    /// <summary>Builds the packet that releases a bag slot.</summary>
    /// <param name="slot">Bag slot.</param>
    /// <returns>The packet.</returns>
    public static byte[] ReleaseSlot(byte slot) => new PacketWriter(Action23.Code, Action23.MagicMapRelease).U8(slot).ToArray();

    /// <summary>Builds the packet that removes items from a bag slot.</summary>
    /// <param name="slot">Bag slot.</param>
    /// <param name="count">Number of items removed.</param>
    /// <returns>The packet.</returns>
    public static byte[] BagSlotDecrease(byte slot, byte count) =>
        new PacketWriter(Action23.Code, Action23.BagSlotDecrease).U8(slot).U8(count).ToArray();

    /// <summary>Builds 23/119 kind 1, the minutes left of the stat buffs.</summary>
    /// <param name="minutes">Minutes left of each of the five rows; missing rows are sent as 0.</param>
    /// <returns>The packet.</returns>
    public static byte[] StatBuffMinutes(IReadOnlyList<ushort> minutes)
    {
        var w = new PacketWriter(Action23.Code, Action23.StatBuff).U8(1);
        for (var i = 0; i < 5; i++)
            w.U16(i < minutes.Count ? minutes[i] : (ushort)0);
        return w.ToArray();
    }

    /// <summary>Builds 23/119 kind 2, the stat buff mask.</summary>
    /// <param name="mask">The buff mask.</param>
    /// <param name="sound">True to play the use sound.</param>
    /// <returns>The packet.</returns>
    public static byte[] StatBuffMask(uint mask, bool sound) =>
        new PacketWriter(Action23.Code, Action23.StatBuff).U8(2).U32(mask).U8(sound ? (byte)1 : (byte)0).ToArray();

    /// <summary>Builds 23/15, which plays the sound of an item being used.</summary>
    /// <returns>The packet.</returns>
    public static byte[] UsedSound() => new PacketWriter(Action23.Code, Action23.UsedSound).ToArray();

    /// <summary>Builds 23/7, which removes items from the bag by item id.</summary>
    /// <param name="itemId">Id of the item.</param>
    /// <param name="count">Number of items removed.</param>
    /// <returns>The packet.</returns>
    public static byte[] BagRemoveById(ushort itemId, byte count) =>
        new PacketWriter(Action23.Code, Action23.BagRemoveById).U16(itemId).U8(count).ToArray();

    /// <summary>Builds 23/17, which equips an item from the bag.</summary>
    /// <param name="bagSlot">Bag slot of the item.</param>
    /// <param name="freeSlot">Bag slot that receives the item previously worn.</param>
    /// <returns>The packet.</returns>
    public static byte[] EquipFromBag(byte bagSlot, byte freeSlot) =>
        new PacketWriter(Action23.Code, Action23.EquipFromBag).U8(bagSlot).U8(freeSlot).ToArray();

    /// <summary>Builds the packet that moves a worn item into the bag.</summary>
    /// <param name="equipSlot">Equipment slot of the item.</param>
    /// <param name="bagSlot">Bag slot that receives it.</param>
    /// <returns>The packet.</returns>
    public static byte[] UnequipToBag(byte equipSlot, byte bagSlot) =>
        new PacketWriter(Action23.Code, Action23.UnequipToBag).U8(equipSlot).U8(bagSlot).ToArray();

    /// <summary>Builds 23/19, which clears a worn item slot.</summary>
    /// <param name="equipSlot">Equipment slot.</param>
    /// <returns>The packet.</returns>
    public static byte[] EquipSlotCleared(byte equipSlot) =>
        new PacketWriter(Action23.Code, Action23.EquipSlotCleared).U8(equipSlot).ToArray();

    /// <summary>Builds the packet that moves items between bag slots.</summary>
    /// <param name="from">Source slot.</param>
    /// <param name="count">Number of items moved.</param>
    /// <param name="to">Target slot.</param>
    /// <returns>The packet.</returns>
    public static byte[] BagMove(byte from, byte count, byte to) =>
        new PacketWriter(Action23.Code, Action23.BagMove).U8(from).U8(count).U8(to).ToArray();

    /// <summary>Builds the packet that tells the client which players started or stopped collecting automatically.</summary>
    /// <param name="playerIds">Ids of the players.</param>
    /// <param name="started">True for 23/138 (started), false for 23/137 (stopped).</param>
    /// <returns>The packet.</returns>
    public static byte[] AutoCollecting(IEnumerable<int> playerIds, bool started)
    {
        var w = new PacketWriter(Action23.Code, started ? Action23.AutoCollectStarted : Action23.AutoCollectStopped);
        foreach (var id in playerIds)
            w.I32(id);
        return w.ToArray();
    }

    /// <summary>Builds the packet that shows another player putting on or taking off an item.</summary>
    /// <param name="playerId">Id of the player.</param>
    /// <param name="itemId">Id of the item.</param>
    /// <param name="on">True for putting on (5/2), false for taking off (5/1).</param>
    /// <returns>The packet.</returns>
    public static byte[] PlayerEquipChange(int playerId, ushort itemId, bool on) =>
        new PacketWriter(Action5.Code, on ? Action5.EquipOn : Action5.EquipOff).I32(playerId).U16(itemId).ToArray();
}
