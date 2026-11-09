using System.Collections.Immutable;
using NorthIsland.GameData.Formulas;
using NorthIsland.Protocol.Messages.Player;
using NorthIsland.Protocol.Opcodes;

namespace NorthIsland.Server.World;

/// <summary>
/// Works out the five combat values (ATK, DEF, MATK, MDEF, SPD) of characters, pets and monsters and sends the character's
/// to the client.
/// </summary>
/// <param name="formulas">The Formula.dat tables.</param>
/// <param name="items">Item.dat, for the effects of worn items.</param>
/// <param name="options">The battle options, for the saddle divisor.</param>
public sealed class CombatStats(
    FormulaData formulas,
    ItemData items,
    Microsoft.Extensions.Options.IOptions<BattleOptions> options)
{
    /// <summary>The index of SPD in the five-value arrays (ATK, DEF, MATK, MDEF, SPD).</summary>
    public const int SpeedIndex = 4;

    /// <summary>The attribute ids (41 to 45) of ATK, DEF, MATK, MDEF and SPD in the set-attribute packet.</summary>
    public const byte AttrAtk = 41, AttrDef = 42, AttrMatk = 43, AttrMdef = 44, AttrSpd = 45;

    /// <summary>The attribute ids of the worn items' max HP bonus and max SP bonus (207, 208).</summary>
    public const byte AttrMaxHpBonus = 207, AttrMaxSpBonus = 208;

    /// <summary>The attribute ids that carry the riding extra of ATK, DEF, MATK, MDEF and SPD, in that order.</summary>
    public static readonly ImmutableArray<byte> RidingExtraAttrs = [57, 51, 52, 53, 54];

    /// <summary>The item attribute ids, in value order, whose worn-item bonus goes into each of the five values.</summary>
    private static readonly ImmutableArray<byte> ItemAttrs = [
        Battle.AttrAttack,
        Battle.AttrDefence,
        Battle.AttrMagicAttack,
        Battle.AttrMagicDefence,
        Battle.AttrSpeed
    ];

    /// <summary>The saddle item ids with the value index each feeds (0 ATK, 1 DEF, 2 MATK, 3 MDEF, 4 SPD).</summary>
    public static readonly ImmutableArray<(ushort Saddle, int Value)> Saddles = [
        (25535, 0), // Bash Saddle
        (25539, 1), // Defence Saddle
        (25536, 2), // Magic Saddle
        (25625, 3), // Spirit Saddle
        (25208, 4)]; // Swift Saddle

    /// <summary>Adds up the bonus of one attribute over the worn items that are not destroyed, with their inserted stone and fusion.</summary>
    /// <param name="equipment">The worn items.</param>
    /// <param name="attr">The item attribute id.</param>
    /// <returns>The total bonus.</returns>
    public int Bonus(IEnumerable<EquippedItem> equipment, byte attr) =>
        equipment.Sum(
            e => !Durability.IsDestroyed(e.Wear) && items[e.ItemId] is { } r ? Gems.Effect(r, e.Insert == 0 ? null : items[e.Insert], attr, e.Fusion) : 0);

    /// <summary>Recalculates the character's derived values, such as max HP and SP, with the max bonuses of the worn items.</summary>
    /// <param name="c">The character.</param>
    public void Recalculate(Character c) => formulas.Recalculate(c, MaxBonus(c, AttrMaxHpBonus), MaxBonus(c, AttrMaxSpBonus));

    /// <summary>The bonus of an attribute from the worn items, plus the character's spring bonus for max HP and max SP.</summary>
    /// <param name="c">The character.</param>
    /// <param name="attr">The attribute id.</param>
    /// <returns>The bonus.</returns>
    public int MaxBonus(Character c, byte attr) => Bonus(c.Equipment, attr) + (attr is AttrMaxHpBonus or AttrMaxSpBonus ? c.SpringBonus : 0);

    /// <summary>The attribute ids whose pet bonuses are sent to the client.</summary>
    private static readonly ImmutableArray<byte> PetBonusAttrs = [
        Battle.AttrAttack,
        Battle.AttrDefence,
        Battle.AttrSpeed,
        Battle.AttrMagicAttack,
        Battle.AttrMagicDefence,
        AttrMaxHpBonus,
        AttrMaxSpBonus
    ];

    /// <summary>Sends the client the worn-item bonus of each attribute for a pet.</summary>
    /// <param name="session">The owner's session.</param>
    /// <param name="pet">The pet.</param>
    public void SendPetBonuses(GameSession session, Pet pet)
    {
        foreach (var attr in PetBonusAttrs)
            session.Send(PetMessages.SetAttribute((byte)pet.Slot, attr, Bonus(pet.Equipment, attr)));
    }

    /// <summary>A pet's five values (ATK, DEF, MATK, MDEF, SPD) from its level, stats, worn items and element.</summary>
    /// <param name="p">The pet.</param>
    /// <returns>The five values; zeros without Formula.dat.</returns>
    public int[] PetValues(Pet p)
    {
        if (formulas.Table is not { } f)
            return new int[5];
        var level = FormulaTable.TotalLevel(p.Level, p.Reborn);
        int[] stats = [p.Strength, p.Constitution, p.Intelligence, p.Wisdom, p.Agility];
        return [
            .. Enumerable.Range(0, 5).Select(i => f.CombatValue(i, level, stats[i], Bonus(p.Equipment, ItemAttrs[i]), ElementMatches(i, p.Element), false))
        ];
    }

    /// <summary>A monster's SPD.</summary>
    /// <param name="npc">The monster's Npc.dat record.</param>
    /// <returns>The SPD value.</returns>
    public int MonsterSpeed(NorthIsland.GameData.Npcs.NpcRecord npc) => MonsterValues(npc)[SpeedIndex];

    /// <summary>A monster's five values (ATK, DEF, MATK, MDEF, SPD) from its Npc.dat level, stats and element.</summary>
    /// <param name="npc">The monster's Npc.dat record.</param>
    /// <returns>The five values; zeros without Formula.dat.</returns>
    public int[] MonsterValues(NorthIsland.GameData.Npcs.NpcRecord npc)
    {
        if (formulas.Table is not { } f)
            return new int[5];
        int[] stats = [npc.Strength, npc.Constitution, npc.Intelligence, npc.Wisdom, npc.Agility];
        return [.. Enumerable.Range(0, 5).Select(i => f.CombatValue(i, npc.Level, stats[i], 0, ElementMatches(i, npc.Element), false))];
    }

    /// <summary>The riding extra of each of the five values: a share of the ridden pet's value for each saddle it wears.</summary>
    /// <param name="ridden">The pet being ridden, null on foot.</param>
    /// <returns>The five extras, all 0 on foot.</returns>
    public int[] RidingExtras(Pet? ridden)
    {
        var extras = new int[5];
        if (ridden is null)
            return extras;
        var pet = PetValues(ridden);
        foreach (var (saddle, value) in Saddles)
            if (ridden.Equipment.Exists(e => e.ItemId == saddle))
                extras[value] = pet[value] / (value is 1 or 3 ? Math.Max(1, options.Value.DefenceSaddleDivisor) : 3);
        return extras;
    }

    /// <summary>Whether the element is the one that boosts a value (3 for ATK and MATK, 1 for DEF and MDEF, 4 for SPD).</summary>
    /// <param name="value">The value index: 0 ATK, 1 DEF, 2 MATK, 3 MDEF, 4 SPD.</param>
    /// <param name="element">The element id.</param>
    /// <returns>True when it boosts the value.</returns>
    private static bool ElementMatches(int value, byte element) => element == (value switch
    {
        0 or 2 => 3, // ATK and MATK: element 3
        1 or 3 => 1, // DEF and MDEF: element 1
        _ => 4 // SPD: element 4
    });

    /// <summary>
    /// A character's five values (ATK, DEF, MATK, MDEF, SPD) from level, stats, worn items, job, element, riding and timed
    /// effects.
    /// </summary>
    /// <param name="c">The character.</param>
    /// <param name="ridden">The pet being ridden, null on foot.</param>
    /// <returns>The five values; zeros without Formula.dat.</returns>
    public int[] Values(Character c, Pet? ridden = null)
    {
        if (formulas.Table is not { } f)
            return new int[5];
        var level = FormulaTable.TotalLevel(c.Level, c.RebornCount);
        var extras = RidingExtras(ridden);
        int[] stats = [c.Strength, c.Constitution, c.Intelligence, c.Wisdom, c.Agility];
        bool Job(int i) => i switch
        {
            0 => c.Job == Career.Killer, // ATK: true for a Killer
            1 => c.Job == Career.Warrior, // DEF: true for a Warrior
            2 => c.Job == Career.Wit, // MATK: true for a Wit
            3 => c.Job == Career.Priest, // MDEF: true for a Priest
            _ => c.Job is Career.Knight or Career.Seer, // SPD: true for a Knight or a Seer
        };
        var now = TimedEffects.Now;
        return [
            .. Enumerable.Range(0, 5)
                .Select(
                    i =>
                        f.CombatValue(
                            i,
                            level,
                            stats[i],
                            Bonus(c.Equipment, ItemAttrs[i]),
                            ElementMatches(i, c.Element),
                            Job(i),
                            extras[i] + TimedEffects.StatBonus(c, i, now)))
        ];
    }

    /// <summary>Finds the pet the session's character is riding.</summary>
    /// <param name="session">The player's session.</param>
    /// <returns>The pet, or null when on foot.</returns>
    public static Pet? Ridden(GameSession session) =>
        session.RidingPetSlot == 0 ? null : session.ActiveCharacter?.Pets.Find(p => p.Slot == session.RidingPetSlot);

    /// <summary>
    /// Recalculates the character's values and sends the client the max bonuses, changed HP and SP, the riding extras and
    /// the five values.
    /// </summary>
    /// <param name="session">The player's session.</param>
    public void Send(GameSession session)
    {
        if (session.ActiveCharacter is not { } c)
            return;
        var (hp, sp) = (c.Hp, c.Sp);
        Recalculate(c);
        foreach (var attr in new[]
        {
            Battle.AttrAttack,
            Battle.AttrDefence,
            Battle.AttrSpeed,
            Battle.AttrMagicAttack,
            Battle.AttrMagicDefence,
            AttrMaxHpBonus,
            AttrMaxSpBonus
        })
            session.Send(InnMessages.SetAttribute(attr, MaxBonus(c, attr)));
        if (c.Hp != hp)
            session.Send(InnMessages.SetAttribute(Action8.AttrHp, c.Hp));
        if (c.Sp != sp)
            session.Send(InnMessages.SetAttribute(Action8.AttrSp, c.Sp));
        var ridden = Ridden(session);
        var extras = RidingExtras(ridden);
        for (var i = 0; i < extras.Length; i++)
            session.Send(InnMessages.SetAttribute(RidingExtraAttrs[i], extras[i]));
        var values = Values(c, ridden);
        for (var i = 0; i < values.Length; i++)
            session.Send(InnMessages.SetAttribute((byte)(AttrAtk + i), values[i]));
    }
}
