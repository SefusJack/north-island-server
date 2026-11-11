using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NorthIsland.GameData.Formulas;
using NorthIsland.Protocol.Messages.Items;
using NorthIsland.Protocol.Messages.Player;
using NorthIsland.Protocol.Opcodes;

namespace NorthIsland.Server.Features.PetCare;

/// <summary>Handles the potential pills: raising a character's or a pet's potential, and cancelling a pet's.</summary>
/// <param name="npcs">The NPC data, to tell pets without a grow key.</param>
/// <param name="items">The item data, for stacking the returned pills.</param>
/// <param name="combat">The combat stats, recalculated after a change to the player or a pet.</param>
/// <param name="options">The item options, which hold the chances.</param>
/// <param name="logger">The logger.</param>
public sealed class PotentialHandlers(
    NpcData npcs,
    ItemData items,
    CombatStats combat,
    IOptions<ItemOptions> options,
    ILogger<PotentialHandlers> logger)
{
    /// <summary>Item id of the Potential Pill, which raises the potential with a bonus chance and costs a level on failure.</summary>
    public const ushort Pill = 34269; // Potential Pill

    /// <summary>Item id of the pill that costs nothing on failure; it works from potential 10.</summary>
    public const ushort NoPenaltyPill = 34289; // Potential  Pill

    /// <summary>Item id of the Super Potential Pill, which always succeeds.</summary>
    public const ushort SuperPill = 34350; // Super Potential Pill

    /// <summary>Item id of the Anti-Potential Pill, which sets a pet's potential back to 0.</summary>
    public const ushort AntiPill = 34351; // Anti-Potential Pill

    /// <summary>The highest potential.</summary>
    public const int MaxPotential = 12;

    /// <summary>The random source for a pill's success, a number from 0 up to 1; tests replace it.</summary>
    public Func<double> Roll { get; set; } = () => GameRandom.Shared.NextDouble();

    /// <summary>Gets the ability points a potential level adds to each ability.</summary>
    /// <param name="level">The potential level, clamped to 0..12.</param>
    /// <returns>The points.</returns>
    private static ushort Step(int level) => (ushort)(ItemEffectHandlers.PotentialFloor[Math.Clamp(level, 0, MaxPotential)]);

    /// <summary>Uses a potential pill on the player or on a pet to raise the potential by one; the pill is used up either way.</summary>
    /// <param name="session">The player's session.</param>
    /// <param name="body">The packet body: the pill's bag slot, then the pet's slot (0 for the player).</param>
    internal ValueTask OnPotential(GameSession session, ReadOnlyMemory<byte> body)
    {
        if (session.State != SessionState.InWorld || session.ActiveCharacter is not { } c || body.Length < 2 || session.Battle is not null)
            return ValueTask.CompletedTask;
        var (slot, petSlot) = (body.Span[0], body.Span[1]);
        var stack = c.Bag.Find(b => b.Slot == slot);
        var pet = petSlot == 0 ? null : c.Pets.Find(p => p.Slot == petSlot);
        if (stack is null || stack.ItemId is not (Pill or NoPenaltyPill or SuperPill) || (petSlot != 0 && pet is null))
        {
            logger.LogInformation("[{Session}] {Name}: potential pill refused (slot {Slot}, pet {Pet})", session.Id, c.Name, slot, petSlot);
            return ValueTask.CompletedTask;
        }
        int level = pet?.Potential ?? c.Potential;
        if (level + 1 > MaxPotential || (stack.ItemId == NoPenaltyPill && level < 10))
        {
            logger.LogInformation("[{Session}] {Name}: potential {Level} cannot take {Item}", session.Id, c.Name, level, stack.ItemId);
            return ValueTask.CompletedTask;
        }
        var o = options.Value.ItemUse;
        var chance = stack.ItemId == SuperPill
            ? 1.0
            : (o.PotentialChances.Length > level ? o.PotentialChances[level] : 0.1) + (stack.ItemId == Pill ? o.PotentialPillBonus : 0);
        var success = chance >= 1.0 || Roll() < chance;
        var newLevel = success ? level + 1 : stack.ItemId == Pill && level > 0 ? level - 1 : level;
        Consume(session, c, stack);
        if (success && pet is not null && npcs[pet.NpcId] is { GrowKey: 0 } && level == 0)
            pet.Tradable = 0;
        Apply(session, c, pet, newLevel, Step(newLevel) - Step(level));
        session.Send(PotentialMessages.Result(petSlot, (byte)newLevel));
        logger.LogInformation(
            "[{Session}] {Name}: potential {Level} -> {New} ({Result}, {Chance:P0})",
            session.Id,
            c.Name,
            level,
            newLevel,
            success ? "success" : "failure",
            chance);
        return ValueTask.CompletedTask;
    }

    /// <summary>Uses an Anti-Potential Pill on a pet: its potential goes to 0 and the player gets Potential Pills back, one per level.</summary>
    /// <param name="session">The player's session.</param>
    /// <param name="body">The packet body: the pill's bag slot, then the pet's slot.</param>
    internal ValueTask OnReturn(GameSession session, ReadOnlyMemory<byte> body)
    {
        if (session.State != SessionState.InWorld || session.ActiveCharacter is not { } c || body.Length < 2 || session.Battle is not null)
            return ValueTask.CompletedTask;
        var (slot, petSlot) = (body.Span[0], body.Span[1]);
        var stack = c.Bag.Find(b => b.Slot == slot);
        var pet = c.Pets.Find(p => p.Slot == petSlot);
        if (stack is null || stack.ItemId != AntiPill || pet is null || pet.Potential == 0)
        {
            logger.LogInformation("[{Session}] {Name}: anti-potential pill refused (slot {Slot}, pet {Pet})", session.Id, c.Name, slot, petSlot);
            return ValueTask.CompletedTask;
        }
        var level = pet.Potential;
        Consume(session, c, stack);
        Apply(session, c, pet, 0, -Step(level));
        session.Send(PotentialMessages.ReturnResult(petSlot));
        foreach (var (bagStack, added) in Bag.Add(c, items, Pill, level))
            session.Send(ItemMessages.BagAdd(new BagEntry { Slot = bagStack.Slot, ItemId = Pill, Count = added, Wear = bagStack.Wear }));
        logger.LogInformation("[{Session}] {Name}: {Pet}'s potential {Level} is cancelled", session.Id, c.Name, pet.Name, level);
        return ValueTask.CompletedTask;
    }

    /// <summary>Sets a new potential level and changes the five abilities by the step, then sends the new values.</summary>
    /// <param name="session">The player's session.</param>
    /// <param name="c">The player's character.</param>
    /// <param name="pet">The pet, or null for the player.</param>
    /// <param name="newLevel">The new potential level.</param>
    /// <param name="change">The points added to (or, when negative, taken from) each ability.</param>
    private void Apply(
        GameSession session,
        Character c,
        Pet? pet,
        int newLevel,
        int change)
    {
        ushort Add(ushort v) => (ushort)Math.Clamp(v + change, 1, ushort.MaxValue);
        if (pet is null)
        {
            c.Potential = (byte)newLevel;
            if (change == 0)
                return;
            (c.Strength, c.Constitution, c.Intelligence, c.Wisdom, c.Agility) = (Add(c.Strength), Add(
                c.Constitution), Add(c.Intelligence), Add(c.Wisdom), Add(c.Agility));
            combat.Recalculate(c);
            foreach (var (attr, v) in new (byte, int)[]
            {
                (Action8.AttrStr, c.Strength), (Action8.AttrCon, c.Constitution), (Action8.AttrInt, c.Intelligence), (Action8.AttrWis, c.Wisdom),
                (Action8.AttrAgi, c.Agility), (Action8.AttrMaxHp, c.MaxHp), (Action8.AttrMaxSp, c.MaxSp), (Action8.AttrHp, c.Hp), (Action8.AttrSp, c.Sp),
            })
                session.Send(InnMessages.SetAttribute(attr, v));
            combat.Send(session);
            return;
        }
        pet.Potential = (byte)newLevel;
        if (change == 0)
            return;
        (pet.Strength, pet.Constitution, pet.Intelligence, pet.Wisdom, pet.Agility) = (Add(pet.Strength), Add(
            pet.Constitution), Add(pet.Intelligence), Add(pet.Wisdom), Add(pet.Agility));
        combat.RecalculatePet(pet);
        foreach (var (attr, v) in new (byte, int)[]
        {
            (Action8.AttrStr, pet.Strength), (Action8.AttrCon, pet.Constitution), (Action8.AttrInt, pet.Intelligence), (Action8.AttrWis, pet.Wisdom),
            (Action8.AttrAgi, pet.Agility), (Action8.AttrMaxHp, pet.MaxHp), (Action8.AttrMaxSp, pet.MaxSp), (Action8.AttrHp, pet.Hp), (Action8.AttrSp, pet.Sp),
        })
            session.Send(PetMessages.SetAttribute(pet.Slot, attr, v));
        combat.SendPetValues(session, pet);
    }

    /// <summary>Uses up one pill of a stack and tells the client.</summary>
    /// <param name="session">The player's session.</param>
    /// <param name="c">The player's character.</param>
    /// <param name="stack">The pill stack.</param>
    private static void Consume(GameSession session, Character c, BagItem stack)
    {
        stack.Count--;
        session.Send(ItemMessages.BagSlotDecrease(stack.Slot, 1));
        if (stack.Count == 0)
            c.Bag.Remove(stack);
        session.Send(ItemMessages.UsedSound());
    }
}
