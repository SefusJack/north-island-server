using NorthIsland.GameData.Items;
using NorthIsland.Protocol.Messages.Items;

namespace NorthIsland.Server.Features.Items;

/// <summary>Equipment wear: when a battle wears gear, by how much, and the wear values that mean broken.</summary>
public static class Durability
{
    /// <summary>The wear value at which a piece is destroyed.</summary>
    public const byte Destroyed = 250;

    /// <summary>The wear value below which the Repairing Spanner can mend a piece.</summary>
    public const byte SpannerMaxWear = 200;

    /// <summary>Whether a piece is destroyed.</summary>
    /// <param name="wear">The piece's wear value.</param>
    /// <returns>True when the wear is 0 or at least <see cref="Destroyed"/>.</returns>
    public static bool IsDestroyed(byte wear) => wear is 0 or >= Destroyed;

    /// <summary>Whether a battle wears gear because of the level gap to the highest monster.</summary>
    /// <param name="playerLevel">The level of the player or pet whose gear is checked.</param>
    /// <param name="highestMonster">The level of the highest monster in the battle, 0 when there was none.</param>
    /// <param name="minGap">The level gap that is needed; 0 or less means every battle wears.</param>
    /// <returns>True when the gear wears.</returns>
    public static bool Wears(int playerLevel, int highestMonster, int minGap) => minGap <= 0 || highestMonster > 0 && highestMonster - playerLevel >= minGap;

    /// <summary>Wears every piece of the player's and the pet's gear by the same amount and sends the new wear values.</summary>
    /// <param name="send">Sends a packet to the player.</param>
    /// <param name="character">The player's character, or null.</param>
    /// <param name="pet">The fighting pet, or null.</param>
    /// <param name="amount">The wear to add to each piece; nothing happens when it is 0 or less.</param>
    public static void AfterBattle(Action<byte[]> send, Character? character, Pet? pet, int amount)
    {
        if (amount > 0)
            AfterBattle(send, character, pet, (_, _) => amount);
    }

    /// <summary>Wears the player's and the pet's gear by an amount chosen per piece and sends the new wear values and any breakage.</summary>
    /// <param name="send">Sends a packet to the player.</param>
    /// <param name="character">The player's character, or null.</param>
    /// <param name="pet">The fighting pet, or null.</param>
    /// <param name="amountOf">Gives the wear for a piece; the flag is true for a pet's piece.</param>
    public static void AfterBattle(Action<byte[]> send, Character? character, Pet? pet, Func<EquippedItem, bool, int> amountOf)
    {
        foreach (var e in character?.Equipment ?? [])
            if (Wear(e, amountOf(e, false)))
            {
                send(ItemMessages.EquipWear(e.Slot, e.Wear));
                if (e.Wear >= Destroyed)
                    send(ItemMessages.EquipBroken(0, e.ItemId));
            }
        if (pet is not null)
            foreach (var e in pet.Equipment)
                if (Wear(e, amountOf(e, true)))
                {
                    send(ItemMessages.PetEquipWear(pet.Slot, e.Slot, e.Wear));
                    if (e.Wear >= Destroyed)
                        send(ItemMessages.EquipBroken(pet.Slot, e.ItemId));
                }
    }

    /// <summary>The wear one battle gives a piece.</summary>
    /// <param name="item">The piece's definition, or null.</param>
    /// <param name="gapReached">Whether the level gap rule (<see cref="Wears"/>) applies.</param>
    /// <param name="o">The wear settings.</param>
    /// <returns>The larger of the piece's fast-wear amount (guns, the Attack Pestle) and the regular wear when the gap is reached.</returns>
    public static int WearOf(ItemRecord? item, bool gapReached, DurabilityOptions o)
    {
        var fast = item switch
        {
            { SubType: Gems.PestleSubType } => o.PestleWearPerBattle, // the Attack Pestle wears at its own rate
            { Type: Gems.GunType } => o.GunWearPerBattle, // guns wear at their own rate
            _ => 0, // any other piece has no fast wear
        };
        return Math.Max(fast, gapReached ? o.WearPerBattle : 0);
    }

    /// <summary>Adds wear to a piece unless it is destroyed already, stopping at <see cref="Destroyed"/>.</summary>
    /// <param name="e">The equipped piece.</param>
    /// <param name="amount">The wear to add.</param>
    /// <returns>True when the piece's wear changed.</returns>
    private static bool Wear(EquippedItem e, int amount)
    {
        if (amount <= 0 || IsDestroyed(e.Wear))
            return false;
        e.Wear = (byte)Math.Min(Destroyed, e.Wear + amount);
        return true;
    }
}
