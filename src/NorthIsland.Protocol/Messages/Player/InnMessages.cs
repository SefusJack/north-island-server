using NorthIsland.Protocol.IO;
using NorthIsland.Protocol.Opcodes;

namespace NorthIsland.Protocol.Messages.Player;

/// <summary>Builders for the inn packets (resting, pet storage) and the packet that sets an own attribute.</summary>
public static class InnMessages
{
    /// <summary>Offers a rest at the inn.</summary>
    /// <param name="price">The price in gold.</param>
    /// <returns>The packet.</returns>
    public static byte[] RestPrice(int price) => new PacketWriter(Action31.Code, Action31.RestPrice).I32(price).ToArray();

    /// <summary>Answers a rest request.</summary>
    /// <param name="answer">The answer code.</param>
    /// <returns>The packet.</returns>
    public static byte[] RestAnswer(byte answer) => new PacketWriter(Action31.Code, Action31.RestAnswer).U8(answer).ToArray();

    /// <summary>Starts the sleep after a rest.</summary>
    /// <returns>The packet.</returns>
    public static byte[] Sleep() => new PacketWriter(Action31.Code, Action31.Sleep).ToArray();

    /// <summary>Number of places in the inn's pet storage.</summary>
    public const int MaxStoredPets = 10;

    /// <summary>Sends the stored pets (<c>31/6</c>), one record per place.</summary>
    /// <param name="stored">Each stored pet with the item ids it wears by equipment slot.</param>
    /// <param name="client">The client generation the records are written for.</param>
    /// <returns>The packet.</returns>
    public static byte[] StorageList(IEnumerable<(PetInfo Pet, ushort[] WornBySlot)> stored, ClientGeneration client)
    {
        var w = new PacketWriter(Action31.Code, Action31.StorageList);
        foreach (var (p, worn) in stored)
        {
            w.U8(p.Slot).U16(p.NpcId).I32(p.Exp).U8(p.Level).Hp(p.Hp, client).U16(p.Sp)
                .U16(p.Intelligence).U16(p.Strength).U16(p.Constitution).U16(p.Agility).U16(p.Wisdom)
                .U8(0).U8(p.Amity).U8(p.Tradable).U16(p.SkillPoints).PStr8(p.Name);
            foreach (var (level, exp) in p.SkillSlots())
                w.U8(level).I32(exp);
            for (var i = 0; i < PetMessages.EquipmentSlots; i++)
                w.U16(i < worn.Length ? worn[i] : (ushort)0);
            w.U8(p.Reborn).U8(p.Potential);
        }
        return w.ToArray();
    }

    /// <summary>Opens the pet storage.</summary>
    /// <returns>The packet.</returns>
    public static byte[] StorageOpen() => new PacketWriter(Action31.Code, Action31.StorageOpen).ToArray();

    /// <summary>Confirms that a pet was stored.</summary>
    /// <param name="idx">The storage place.</param>
    /// <param name="petSlot">The pet's slot in the team.</param>
    /// <returns>The packet.</returns>
    public static byte[] Stored(byte idx, byte petSlot) => new PacketWriter(Action31.Code, Action31.Store).U8(idx).U8(petSlot).ToArray();

    /// <summary>Confirms that a pet was taken out of storage.</summary>
    /// <param name="idx">The storage place.</param>
    /// <param name="petSlot">The pet's slot in the team.</param>
    /// <returns>The packet.</returns>
    public static byte[] Retrieved(byte idx, byte petSlot) => new PacketWriter(Action31.Code, Action31.RetrievedOld).U8(idx).U8(petSlot).ToArray();

    /// <summary>Empties one storage place.</summary>
    /// <param name="idx">The storage place.</param>
    /// <returns>The packet.</returns>
    public static byte[] StorageCleared(byte idx) => new PacketWriter(Action31.Code, Action31.Swap).U8(idx).ToArray();

    /// <summary>Ends a storage action.</summary>
    /// <returns>The packet.</returns>
    public static byte[] StorageDone() => new PacketWriter(Action31.Code, Action31.StorageDone).ToArray();

    /// <summary>Refuses a storage action because of a pet the player already has.</summary>
    /// <param name="storerHasIt">True when the storing player has the pet, false for the other sub code.</param>
    /// <returns>The packet.</returns>
    public static byte[] StorageRefused(bool storerHasIt) =>
        new PacketWriter(Action31.Code, storerHasIt ? Action31.StorerHasPet : Action31.SamePet).ToArray();

    /// <summary>Sets an attribute of the player's own character (<see cref="Action8.SetAttribute"/>).</summary>
    /// <param name="attrId">The attribute id.</param>
    /// <param name="value">The new value; the sign is sent as a separate flag.</param>
    /// <param name="value2">A second value, for example the skill id.</param>
    /// <returns>The packet.</returns>
    public static byte[] SetAttribute(byte attrId, int value, int value2 = 0) =>
        new PacketWriter(Action8.Code, Action8.SetAttribute).U8(attrId).U8((byte)(value < 0 ? 2 : 1)).I32(Math.Abs(value)).I32(value2).ToArray();
}
