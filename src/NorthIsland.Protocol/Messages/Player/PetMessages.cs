using NorthIsland.Protocol.IO;
using NorthIsland.Protocol.Opcodes;

namespace NorthIsland.Protocol.Messages.Player;

/// <summary>Builders for the pet packets, all server to client.</summary>
public static class PetMessages
{
    /// <summary>Number of equipment slots of a pet.</summary>
    public const int EquipmentSlots = 6;

    /// <summary>Size in bytes of one pet equipment record.</summary>
    public const int EquipmentRecordSize = 19;

    /// <summary>A pet joins the player's team (<c>15/1</c>).</summary>
    /// <param name="ownerId">Id of the owner.</param>
    /// <param name="pet">The pet.</param>
    /// <param name="worn">An item the pet already wears (a returning team NPC's weapon), which the client puts on it; 0 for none.</param>
    /// <returns>The packet.</returns>
    public static byte[] Joined(int ownerId, PetInfo pet, ushort worn = 0)
    {
        var w = new PacketWriter(Action15.Code, Action15.PetJoined).I32(ownerId).I32(pet.NpcId).U8(pet.Tradable)
            .U16(pet.Strength).U16(pet.Constitution).U16(pet.Intelligence).U16(pet.Wisdom).U16(pet.Agility)
            .U8(pet.Level).I32(pet.Exp);
        foreach (var (level, exp) in pet.SkillSlots())
            w.U8(level).I32(exp);
        return w.U8(pet.Amity).U16(worn).U8(0).U8(pet.Reborn).U8(pet.Potential).ToArray();
    }

    /// <summary>Sends the player's pet list (<c>15/8</c>), one record per pet.</summary>
    /// <param name="pets">The pets.</param>
    /// <param name="client">The client generation the records are written for.</param>
    /// <returns>The packet.</returns>
    public static byte[] List(IEnumerable<PetInfo> pets, ClientGeneration client)
    {
        var w = new PacketWriter(Action15.Code, Action15.PetList);
        foreach (var p in pets)
        {
            w.U8(p.Slot).U16(p.NpcId).I32(p.Exp).U8(p.Level).Hp(p.Hp, client).U16(p.Sp)
                .U16(p.Intelligence).U16(p.Strength).U16(p.Constitution).U16(p.Agility).U16(p.Wisdom)
                .U8(0).U8(p.Amity).U8(p.Tradable).U16(p.SkillPoints);
            w.PStr8(p.Name);
            foreach (var (level, exp) in p.SkillSlots())
                w.U8(level).I32(exp);
            foreach (var e in p.Equipment.Take(EquipmentSlots))
                e.WriteTo(w);
            w.Bytes(new byte[(EquipmentSlots - Math.Min(EquipmentSlots, p.Equipment.Count)) * EquipmentRecordSize]);
            w.U8(0).U8(0).U8(p.Reborn).U8(p.Potential);
        }
        return w.ToArray();
    }

    /// <summary>Sets an attribute of the pet in a slot (<see cref="Action8.PetAttribute"/>).</summary>
    /// <param name="slot">The pet's slot.</param>
    /// <param name="attrId">The attribute id.</param>
    /// <param name="value">The new value; the sign is sent as a separate flag.</param>
    /// <param name="value2">A second value, for example the skill id.</param>
    /// <returns>The packet.</returns>
    public static byte[] SetAttribute(byte slot, byte attrId, int value, int value2 = 0) =>
        new PacketWriter(Action8.Code, Action8.PetAttribute).U8(4).U16(slot).U8(attrId).U8((byte)(value < 0 ? 2 : 1))
            .I32(Math.Abs(value)).I32(value2).ToArray();

    /// <summary>Makes a pet the active one (<c>19/1</c>).</summary>
    /// <param name="npcId">Npc id of the pet.</param>
    /// <returns>The packet.</returns>
    public static byte[] Active(ushort npcId) => new PacketWriter(Action19.Code, Action19.JoinCombat).I32(npcId).ToArray();

    /// <summary>Sends the active pet to rest.</summary>
    /// <returns>The packet.</returns>
    public static byte[] Rest() => new PacketWriter(Action19.Code, Action19.Rest).ToArray();

    /// <summary>Tells which pet is active at login (<c>19/4</c>).</summary>
    /// <param name="npcId">Npc id of the pet.</param>
    /// <returns>The packet.</returns>
    public static byte[] Selected(ushort npcId) => new PacketWriter(Action19.Code, Action19.Selected).I32(npcId).ToArray();

    /// <summary>Shows another player's active pet (<c>15/4</c>).</summary>
    /// <param name="playerId">Id of the owner.</param>
    /// <param name="npcId">Npc id of the pet.</param>
    /// <param name="tradable">Tradable flag byte.</param>
    /// <param name="name">The pet's name.</param>
    /// <param name="item">Item id the pet wears, 0 for none.</param>
    /// <returns>The packet.</returns>
    public static byte[] ScenePet(
        int playerId,
        ushort npcId,
        byte tradable,
        string name,
        ushort item = 0) =>
            new PacketWriter(Action15.Code, Action15.ScenePets).I32(playerId).I32(npcId).U8(0).U8(tradable).PStr8(name).U16(item).ToArray();

    /// <summary>Shows the item a pet carries.</summary>
    /// <param name="playerId">Id of the owner.</param>
    /// <param name="item">The item id.</param>
    /// <returns>The packet.</returns>
    public static byte[] ItemShown(int playerId, ushort item) => new PacketWriter(Action23.Code, Action23.PetItemShown).I32(playerId).U16(item).ToArray();

    /// <summary>Hides the item a pet carries.</summary>
    /// <param name="playerId">Id of the owner.</param>
    /// <returns>The packet.</returns>
    public static byte[] ItemHidden(int playerId) => new PacketWriter(Action23.Code, Action23.PetItemHidden).I32(playerId).ToArray();

    /// <summary>Removes a player's pet from the scene (<c>19/7</c>).</summary>
    /// <param name="playerId">Id of the owner.</param>
    /// <returns>The packet.</returns>
    public static byte[] ScenePetHidden(int playerId) => new PacketWriter(Action19.Code, Action19.PetHidden).I32(playerId).ToArray();

    /// <summary>Tells that a player mounted a pet.</summary>
    /// <param name="slot">The pet's slot.</param>
    /// <param name="humanId">Id of the rider.</param>
    /// <param name="npcId">Npc id of the pet.</param>
    /// <returns>The packet.</returns>
    public static byte[] Mounted(byte slot, int humanId, ushort npcId) =>
        new PacketWriter(Action15.Code, Action15.PetMounted).U8(slot).I32(humanId).I32(npcId).ToArray();

    /// <summary>Lists the riders of a scene (<c>23/116</c>); entries whose pet slot is not 1 to 4 are left out.</summary>
    /// <param name="riders">Pet slot, rider id and npc id of each rider.</param>
    /// <returns>The packet.</returns>
    public static byte[] RideList(IEnumerable<(byte Slot, int HumanId, ushort NpcId)> riders)
    {
        var w = new PacketWriter(Action23.Code, Action23.RidePetList);
        foreach (var (slot, humanId, npcId) in riders)
            if (slot is >= 1 and <= 4)
                w.U8(slot).I32(humanId).U16(npcId);
        return w.ToArray();
    }

    /// <summary>Tells that a player got off a pet.</summary>
    /// <param name="humanId">Id of the rider.</param>
    /// <returns>The packet.</returns>
    public static byte[] Dismounted(int humanId) => new PacketWriter(Action15.Code, Action15.PetDismounted).I32(humanId).ToArray();

    /// <summary>Moves a bag item onto a pet (<c>23/23</c>).</summary>
    /// <param name="petSlot">The pet's slot.</param>
    /// <param name="bagSlot">The bag slot of the item.</param>
    /// <returns>The packet.</returns>
    public static byte[] EquipFromBag(byte petSlot, byte bagSlot) =>
        new PacketWriter(Action23.Code, Action23.PetEquipFromBag).U8(petSlot).U8(bagSlot).ToArray();

    /// <summary>Moves an item a pet wears into the bag (<c>23/22</c>).</summary>
    /// <param name="petSlot">The pet's slot.</param>
    /// <param name="equipSlot">The pet's equipment slot.</param>
    /// <param name="bagSlot">The bag slot it goes to.</param>
    /// <returns>The packet.</returns>
    public static byte[] UnequipToBag(byte petSlot, byte equipSlot, byte bagSlot) =>
        new PacketWriter(Action23.Code, Action23.PetUnequipToBag).U8(petSlot).U8(equipSlot).U8(bagSlot).ToArray();

    /// <summary>Clears an equipment slot of a pet (<c>23/21</c>).</summary>
    /// <param name="petSlot">The pet's slot.</param>
    /// <param name="equipSlot">The pet's equipment slot.</param>
    /// <returns>The packet.</returns>
    public static byte[] EquipCleared(byte petSlot, byte equipSlot) =>
        new PacketWriter(Action23.Code, Action23.PetEquipCleared).U8(petSlot).U8(equipSlot).ToArray();

    /// <summary>Removes a pet from the team (<c>15/2</c>).</summary>
    /// <param name="ownerId">Id of the owner.</param>
    /// <param name="slot">The pet's slot.</param>
    /// <returns>The packet.</returns>
    public static byte[] Removed(int ownerId, byte slot) => new PacketWriter(Action15.Code, Action15.PetRemoved).I32(ownerId).U8(slot).ToArray();

    /// <summary>Renames a pet (<c>15/9</c>).</summary>
    /// <param name="ownerId">Id of the owner.</param>
    /// <param name="slot">The pet's slot.</param>
    /// <param name="name">The new name.</param>
    /// <returns>The packet.</returns>
    public static byte[] Renamed(int ownerId, byte slot, string name) =>
        new PacketWriter(Action15.Code, Action15.PetRenamed).I32(ownerId).U8(slot).RawString(name).ToArray();
}
