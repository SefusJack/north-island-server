using System.Collections.Immutable;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NorthIsland.Protocol.Messages.Items;

namespace NorthIsland.Server.Features.Crafting;

/// <summary>Handles the Chaos Crystal request, which raises the stone mounted in a piece of bag equipment by one step.</summary>
/// <param name="items">The item data.</param>
/// <param name="options">The item options; the failure loss of the plain Chaos Crystal comes from them.</param>
/// <param name="logger">The log.</param>
public sealed class ConfuseStoneHandlers(
    ItemData items,
    IOptions<ItemOptions> options,
    ILogger<ConfuseStoneHandlers> logger)
{
    /// <summary>Item ids of the Chaos Crystal, the Shadow Chaos Crystal and the Pure Chaos Crystal.</summary>
    public const ushort ChaosCrystal = 61062, // Chaos Crystal
        ShadowChaosCrystal = 61079, // Shadow Chaos Crystal
        PureChaosCrystal = 61080; // Pure Chaos Crystal

    /// <summary>The stones of the 16 steps in order, each with its chance in percent of being reached from the step below.</summary>
    public static readonly ImmutableArray<(ushort Stone, int Percent)> Steps =
        [
            (61063, 100), // Morning Star Crystal
            (61064, 100), // Malefic Crystal
            (61065, 100), // Orange Crystal
            (61066, 95), // Forest Crystal
            (61067, 89), // Purple Crystal
            (61068, 60), // Colorful Crystal
            (61069, 35), // Blizzard Crystal
            (61070, 20), // Purple Haze Crystal
            (61071, 20), // White Crystal
            (61072, 20), // Cyan Crystal
            (61073, 15), // Ultra Violet Crystal
            (61074, 14), // Dark Shadow Crystal
            (61075, 13), // Green Crystal
            (61076, 12), // Yellow Crystal
            (61077, 11), // Blue Crystal
            (61078, 10), // Charm Crystal
        ];

    /// <summary>Random source: returns a number from 0 up to but not including 1. Replaceable by tests.</summary>
    public Func<double> Roll { get; set; } = () => GameRandom.Shared.NextDouble();

    /// <summary>Looks up the step of a mounted stone.</summary>
    /// <param name="stone">The item id of the stone.</param>
    /// <returns>The step from 1 to 16, or 0 when the stone is none of the step stones.</returns>
    private static int StepOf(ushort stone) => Steps.Select(s => s.Stone).ToList().IndexOf(stone) + 1;

    /// <summary>Handles <c>23/127</c>: rolls the next step for the stone in a piece of equipment, spends the crystal and sends the result.</summary>
    /// <param name="session">The player's session.</param>
    /// <param name="body">The packet body: the equipment bag slot, then the crystal bag slot.</param>
    internal ValueTask OnUse(GameSession session, ReadOnlyMemory<byte> body)
    {
        if (session.State != SessionState.InWorld || session.ActiveCharacter is not { } c || body.Length < 2 || session.Battle is not null)
            return ValueTask.CompletedTask;
        var (gearSlot, stoneSlot) = (body.Span[0], body.Span[1]);
        var gear = c.Bag.Find(b => b.Slot == gearSlot);
        var crystal = c.Bag.Find(b => b.Slot == stoneSlot);
        var record = gear is null ? null : items[gear.ItemId];
        if (gear is null || crystal is null || crystal.ItemId is not (ChaosCrystal or ShadowChaosCrystal or PureChaosCrystal) || record is null
            || record.EquipSlot is < 1 or > 6
            || record.Type == Gems.GunType
            || (record.EquipSlot == Gems.AccessorySlot && record.SubType != Gems.ManteauSubType)
            || (gear.Insert != 0 && StepOf(gear.Insert) == 0) || StepOf(gear.Insert) >= Steps.Length)
        {
            logger.LogInformation(
                "[{Session}] {Name}: chaos crystal refused (gear slot {Gear}, crystal slot {Crystal})",
                session.Id,
                c.Name,
                gearSlot,
                stoneSlot);
            return ValueTask.CompletedTask;
        }
        var step = StepOf(gear.Insert);
        var target = step + 1;
        if (Roll() * 100 < Steps[target - 1].Percent)
            step = target;
        else if (crystal.ItemId == ChaosCrystal)
            step = Math.Max(0, step - options.Value.ItemUse.ChaosCrystalFailureLoss);
        gear.Insert = step == 0 ? (ushort)0 : Steps[step - 1].Stone;
        crystal.Count--;
        session.Send(ItemMessages.BagSlotDecrease(crystal.Slot, 1));
        if (crystal.Count == 0)
            c.Bag.Remove(crystal);
        session.Send(ConfuseMessages.Result((byte)step, gear.Slot, gear.Insert));
        logger.LogInformation("[{Session}] {Name}: {Item} stone step {Step}", session.Id, c.Name, record.Name.Trim(), step);
        return ValueTask.CompletedTask;
    }
}
