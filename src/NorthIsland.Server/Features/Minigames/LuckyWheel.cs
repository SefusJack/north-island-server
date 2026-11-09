using System.Collections.Immutable;

namespace NorthIsland.Server.Features.Minigames;

/// <summary>The Lucky Wheel game: the sport kind, the prize tiers, the price and the draws.</summary>
public static class LuckyWheel
{
    /// <summary>Sport kind of the Lucky Wheel, and the number of slots on a wheel.</summary>
    public const byte Kind = 21, Slots = 20;

    /// <summary>The price of a spin in item-mall points and in tokens.</summary>
    public const int Points = 65, Tokens = 13;

    /// <summary>The prizes of the four tiers (item and count), from the best tier to the most common one.</summary>
    public static readonly ImmutableArray<ImmutableArray<(ushort Item, byte Count)>> Tiers =
        [
            [(34238, 1), // Seraph Scroll
                (34165, 1), // Win Plate
                (30296, 1), // Holy Pegasus Voucher
                (21172, 1), // Fur Clothes (Male)
                (21173, 1)], // Fur Clothes(Female)
            [(34189, 1), // 8X Holy EXP Potion
                (34243, 1), // Cherubim Scroll
                (34244, 1), // Ofanim Scroll
                (34245, 1), // Virtues Scroll
                (34247, 1), // Small Demon Scroll
                (25571, 1), // Swift Source
                (25570, 1), // Curse Source
                (25569, 1), // Purple Source
                (25568, 1), // Defence Source
                (25567, 1)], // Energy Source
            [(25164, 1), // Fierce Armlet
                (25166, 1), // Rapid Armlet
                (25168, 1), // War Armlet
                (25170, 1), // Bright Armlet
                (34104, 1), // Repairing Spanner
                (25176, 1), // Power Armlet
                (25177, 1), // Defensive Armlet
                (25178, 1), // Brilliant Armlet
                (25179, 1), // Original Armlet
                (25180, 1)], // Fast Armlet
            [(32162, 1), // Pizza
                (33031, 2), // Phoenix Blood
                (34022, 1), // Tent Palette
                (34015, 1), // Glazed Stone Bag
                (34157, 1), // Cherry Door Voucher
                (34158, 1), // Beach Door Voucher
                (34175, 1), // Seasonal Suit
                (34176, 1), // Rabbit Suit
                (34177, 1), // Cat Suit
                (25186, 1), // Spirit Satin
                (25187, 1), // Cotton Scroll
                (25188, 1), // Red Scroll
                (25189, 1), // Mysterious Scroll
                (25190, 1), // Swift Scroll
                (25154, 1), // Anger Armlet
                (25156, 1), // Fortitude Armlet
                (25158, 1), // Sage Armlet
                (25160, 1), // Scale Armlet
                (25162, 1), // Swift Armlet
                (34137, 1)], // Pet Lethe Scroll
        ];

    /// <summary>Builds the code of a prize on the wheel.</summary>
    /// <param name="tier">The tier, 0..3.</param>
    /// <param name="row">The 1-based row in the tier's prize list.</param>
    /// <returns>The code, <c>tier * 30 + row</c>.</returns>
    public static byte Code(int tier, int row) => (byte)(tier * 30 + row);

    /// <summary>Looks up the prize behind a code.</summary>
    /// <param name="code">The prize code.</param>
    /// <returns>The tier, the item id and the item count.</returns>
    public static (int Tier, ushort Item, byte Count) Prize(byte code)
    {
        var tier = Math.Min(3, (code - 1) / 30);
        var (item, count) = Tiers[tier][code - tier * 30 - 1];
        return (tier, item, count);
    }

    /// <summary>How many prizes of each tier a new wheel carries.</summary>
    private static readonly ImmutableArray<int> PerTier = [1, 2, 3, 14];

    /// <summary>The weight of a slot of each tier when a spin picks the stopping slot; a higher tier number weighs more.</summary>
    private static readonly ImmutableArray<int> Weight = [1, 4, 8, 20];

    /// <summary>Builds a new wheel: random distinct prizes per tier, placed in random slots.</summary>
    /// <param name="roll">Random source: <c>roll(n)</c> returns an integer from 0 to n - 1.</param>
    /// <returns>The prize code of each of the 20 slots.</returns>
    public static byte[] NewWheel(Func<int, int> roll)
    {
        List<byte> codes = [];
        for (var tier = 0; tier < Tiers.Length; tier++)
        {
            var rows = Enumerable.Range(1, Tiers[tier].Length).ToList();
            for (var n = 0; n < PerTier[tier]; n++)
            {
                var pick = roll(rows.Count);
                codes.Add(Code(tier, rows[pick]));
                rows.RemoveAt(pick);
            }
        }
        var wheel = new byte[Slots];
        for (var slot = 0; slot < Slots; slot++)
        {
            var pick = roll(codes.Count);
            wheel[slot] = codes[pick];
            codes.RemoveAt(pick);
        }
        return wheel;
    }

    /// <summary>Picks the slot a spin stops at, weighting each slot by the tier of its prize.</summary>
    /// <param name="wheel">The prize codes of the slots.</param>
    /// <param name="roll">Random source: <c>roll(n)</c> returns an integer from 0 to n - 1.</param>
    /// <returns>The slot, 1-based.</returns>
    public static byte Spin(byte[] wheel, Func<int, int> roll)
    {
        var total = wheel.Sum(c => Weight[Prize(c).Tier]);
        var pick = roll(total);
        for (var slot = 0; slot < wheel.Length; slot++)
        {
            pick -= Weight[Prize(wheel[slot]).Tier];
            if (pick < 0)
                return (byte)(slot + 1);
        }
        return (byte)wheel.Length;
    }
}
