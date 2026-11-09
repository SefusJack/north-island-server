using Microsoft.Extensions.Logging;
using NorthIsland.GameData.Items;
using NorthIsland.Protocol.Messages.Items;
using NorthIsland.Protocol.Messages.Player;
using NorthIsland.Protocol.Opcodes;

namespace NorthIsland.Server.Features.Items;

/// <summary>
/// HP and SP potions that fill the bottle pools, and the HP and SP bottles that pay from the pool: using one on the player or a pet restores
/// the attribute, dragging a potion onto a bottle pours it in.
/// </summary>
/// <param name="items">Item definitions, to tell bottle kinds and capacities.</param>
/// <param name="logger">Logger.</param>
public sealed class BottleHandlers(
    ItemData items,
    ILogger<BottleHandlers> logger)
{
    /// <summary>At login, sends the HP and SP bottle pools that are not empty.</summary>
    /// <param name="session">The player's session.</param>
    /// <param name="c">The player's character.</param>
    public static void SendOnLogin(GameSession session, Character c)
    {
        if (c.HpBottle > 0)
            session.Send(BottleMessages.Pool(3, c.HpBottle)); // kind
        if (c.SpBottle > 0)
            session.Send(BottleMessages.Pool(4, c.SpBottle)); // kind
    }

    /// <summary>
    /// Uses a potion or bottle: restores HP or SP of the player or the pet up to the maximum, from the potion's unused part (kinds 1 and 2) or
    /// from the bottle pool (kinds 3 and 4).
    /// </summary>
    /// <param name="session">The player's session.</param>
    /// <param name="c">The player's character.</param>
    /// <param name="stack">The bag stack that was used.</param>
    /// <param name="item">The definition of the item.</param>
    /// <param name="pet">The pet to heal, or null for the player.</param>
    /// <returns>True when the item is a potion or bottle and was handled, even if nothing was restored; false for any other item.</returns>
    public bool TryUse(
        GameSession session,
        Character c,
        BagItem stack,
        ItemRecord item,
        Pet? pet)
    {
        var kind = Bottles.KindOf(item);
        if (kind == 0)
            return false;
        var hp = kind is 1 or 3;
        var (cur, max) = pet is null ? (hp ? c.Hp : c.Sp, hp ? c.MaxHp : c.MaxSp) : (hp ? pet.Hp : pet.Sp, hp ? pet.MaxHp : pet.MaxSp);
        var need = max - cur;
        if (need <= 0)
            return true;
        int gain;
        if (kind <= 2)
        {
            gain = Math.Min(need, Bottles.Capacity(item) - stack.Used);
            if (gain <= 0)
                return true;
        }
        else
        {
            gain = Math.Min(need, hp ? c.HpBottle : c.SpBottle);
            if (gain <= 0)
                return true;
            if (hp)
                c.HpBottle -= gain;
            else
                c.SpBottle -= gain;
            session.Send(BottleMessages.Pool(kind, hp ? c.HpBottle : c.SpBottle));
        }
        Apply(session, c, pet, hp, cur + gain);
        if (kind <= 2)
        {
            stack.Used += gain;
            session.Send(BottleMessages.Used(stack.Slot, gain));
            if (stack.Used >= Bottles.Capacity(item))
                Remove(session, c, stack);
        }
        session.Send(ItemMessages.UsedSound());
        logger.LogInformation("[{Session}] {Name} uses {Item}: {Attr} +{Gain}", session.Id, c.Name, item.Name, hp ? "HP" : "SP", gain);
        return true;
    }

    /// <summary>Pours a potion into a bottle of its kind (HP into the HP bottle, SP into the SP bottle), up to the pool maximum.</summary>
    /// <param name="session">The player's session.</param>
    /// <param name="c">The player's character.</param>
    /// <param name="source">The potion stack that is poured.</param>
    /// <param name="target">The bottle stack it was dropped on.</param>
    /// <returns>True when the pair is a potion and a bottle of the same attribute and was handled; false otherwise.</returns>
    public bool TryFill(GameSession session, Character c, BagItem source, BagItem target)
    {
        if (items[source.ItemId] is not { } from || items[target.ItemId] is not { } to)
            return false;
        var (fromKind, toKind) = (Bottles.KindOf(from), Bottles.KindOf(to));
        if (!(fromKind == 1 && toKind == 3 || fromKind == 2 && toKind == 4))
            return false;
        var hp = toKind == 3;
        var pool = hp ? c.HpBottle : c.SpBottle;
        var pour = Math.Min(Bottles.Capacity(from) - source.Used, Bottles.PoolMax - pool);
        if (pour <= 0)
            return true;
        pool += pour;
        if (hp)
            c.HpBottle = pool;
        else
            c.SpBottle = pool;
        source.Used += pour;
        session.Send(BottleMessages.Used(source.Slot, pour));
        session.Send(BottleMessages.Pool(fromKind, pool));
        if (source.Used >= Bottles.Capacity(from))
            Remove(session, c, source);
        logger.LogInformation("[{Session}] {Name} pours {Amount} into the {Bottle}: {Pool}", session.Id, c.Name, pour, to.Name, pool);
        return true;
    }

    /// <summary>Sets the player's or the pet's HP or SP to a new value and tells the client.</summary>
    /// <param name="session">The player's session.</param>
    /// <param name="c">The player's character.</param>
    /// <param name="pet">The pet to set, or null for the player.</param>
    /// <param name="hp">True for HP, false for SP.</param>
    /// <param name="value">The new value.</param>
    private static void Apply(
        GameSession session,
        Character c,
        Pet? pet,
        bool hp,
        int value)
    {
        if (pet is null)
        {
            if (hp)
                c.Hp = value;
            else
                c.Sp = value;
            session.Send(InnMessages.SetAttribute(hp ? Action8.AttrHp : Action8.AttrSp, value));
        }
        else
        {
            if (hp)
                pet.Hp = value;
            else
                pet.Sp = value;
            session.Send(PetMessages.SetAttribute(pet.Slot, hp ? Action8.AttrHp : Action8.AttrSp, value));
        }
    }

    /// <summary>Removes a used-up potion from the bag and tells the client.</summary>
    /// <param name="session">The player's session.</param>
    /// <param name="c">The player's character.</param>
    /// <param name="stack">The stack to remove.</param>
    private static void Remove(GameSession session, Character c, BagItem stack)
    {
        session.Send(ItemMessages.BagSlotDecrease(stack.Slot, 1));
        c.Bag.Remove(stack);
    }
}
