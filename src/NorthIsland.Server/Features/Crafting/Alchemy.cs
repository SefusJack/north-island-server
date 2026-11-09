using NorthIsland.GameData.Items;

namespace NorthIsland.Server.Features.Crafting;

/// <summary>
/// The rules of alchemy compounding: which items can be made, which material classes combine, and how the product rank is rolled.
/// </summary>
public static class Alchemy
{
    /// <summary>Item id of the common stone a failed compound turns into.</summary>
    public const ushort CommonStone = 43001; // Common Stone

    /// <summary>Item sub type of the alchemy books.</summary>
    public const byte BookSubType = 141;

    /// <summary>Tells whether an item is an alchemy book.</summary>
    /// <param name="item">The item record to test.</param>
    /// <returns>True when the item has the alchemy book sub type.</returns>
    public static bool IsBook(ItemRecord item) => item.SubType == BookSubType;

    /// <summary>The class value every piece of equipment carries in the item data; <see cref="Class"/> reads it as no class.</summary>
    public const ushort EquipmentClass = 250;

    /// <summary>
    /// Item type of the Bless Pack items (<see cref="BlessItemType"/>) and item sub type of the team NPCs' gear
    /// (<see cref="BlessGearSubType"/>); both only go into a bless compound.
    /// </summary>
    public const byte BlessItemType = 20, BlessGearSubType = 149;

    /// <summary>Tells whether an item only goes into a bless compound.</summary>
    /// <param name="item">The item record to test.</param>
    /// <returns>True when the item is a Bless Pack item or a piece of team NPC gear.</returns>
    public static bool IsBlessItem(ItemRecord item) => item.Type == BlessItemType || item.SubType == BlessGearSubType;

    /// <summary>Tells whether the items of a bless compound are all of one kind.</summary>
    /// <param name="chosen">The item records of the chosen stacks.</param>
    /// <returns>True when all are Bless Pack items or all are team NPC gear.</returns>
    public static bool SameBlessKind(IReadOnlyList<ItemRecord> chosen) =>
        chosen.All(i => i.Type == BlessItemType) || chosen.All(i => i.SubType == BlessGearSubType);

    /// <summary>Reads one of the two material classes of an item; the equipment class counts as no class.</summary>
    /// <param name="item">The item record.</param>
    /// <param name="i">Which class to read, 0 for the first and 1 for the second.</param>
    /// <returns>The class number, or 0 when the item has none at that position.</returns>
    public static ushort Class(ItemRecord item, int i) =>
        item.Words142.Length > i && item.Words142[i] != EquipmentClass ? item.Words142[i] : (ushort)0;

    /// <summary>Tells whether an item can come out of a compound.</summary>
    /// <param name="item">The item record to test.</param>
    /// <returns>
    /// True when the item has a shop rank, a first class, a second class other than 67, is not a book and has a printable name.
    /// </returns>
    public static bool CanBeMade(ItemRecord item) =>
        item.ShopColumn >= 1 && Class(item, 0) != 0 && Class(item, 1) != 67 && !IsBook(item)
            && item.Name.Trim().Length > 0 && item.Name.All(ch => ch is >= ' ' and <= '~');

    /// <summary>Works out how many ranks above the lowest material a compound reaches.</summary>
    /// <param name="level">The compound level: skill level plus the rank of the book used.</param>
    /// <param name="roll">A random number from 0 to 99.</param>
    /// <returns>The level plus 3 for a roll below 1, plus 1 for a roll below 70, otherwise plus 2.</returns>
    public static int Jump(int level, int roll) => roll < 1 ? level + 3 : roll < 70 ? level + 1 : level + 2;

    /// <summary>
    /// Rolls the result of compounding: the highest rank in reach that the materials' classes allow, or a failure when no item qualifies.
    /// </summary>
    /// <param name="materials">The item records of the materials, books left out.</param>
    /// <param name="level">The compound level: skill level plus the rank of the book used.</param>
    /// <param name="all">Every item of the game data, searched for the product.</param>
    /// <param name="roll">Random source: returns a number from 0 up to but not including its argument.</param>
    /// <returns>The outcome, the product (only for a made item) and the target rank that was rolled.</returns>
    public static AlchemyResult Compound(IReadOnlyList<ItemRecord> materials, int level, IEnumerable<ItemRecord> all, Func<int, int> roll)
    {
        var classes = materials.Select(m => Class(m, 0)).Where(c => c != 0).ToHashSet();
        var baseRank = materials.Min(m => m.ShopColumn);
        var target = baseRank + Jump(level, roll(100));
        var pool = all.Where(i => CanBeMade(i) && i.ShopColumn > baseRank && i.ShopColumn <= target
            && classes.Contains(Class(i, 0)) && (Class(i, 1) == 0 || classes.Contains(Class(i, 1))))
            .GroupBy(i => i.ShopColumn).MaxBy(g => g.Key)?.OrderBy(i => i.Id).ToList();
        if (pool is { Count: > 0 })
            return new AlchemyResult(AlchemyOutcome.Made, pool[roll(pool.Count)], target);
        var fail = roll(100);
        return new AlchemyResult(fail < 80 ? AlchemyOutcome.NothingLost : fail < 90 ? AlchemyOutcome.Degraded : AlchemyOutcome.Stone, null, target);
    }

    /// <summary>Finds the item a failed compound degrades to: a lower-rank item of the same class as the main material.</summary>
    /// <param name="main">The item record of the main (first) material.</param>
    /// <param name="all">Every item of the game data.</param>
    /// <returns>
    /// The item at the highest rank up to three below the main material's (rank 1 at the least), preferring the main material's type;
    /// null when none exists.
    /// </returns>
    public static ItemRecord? Degraded(ItemRecord main, IEnumerable<ItemRecord> all)
    {
        var rank = Math.Max(1, main.ShopColumn - 3);
        return all.Where(i => CanBeMade(i) && Class(i, 0) == Class(main, 0) && i.ShopColumn <= rank && i.Id != main.Id)
            .OrderByDescending(i => i.ShopColumn).ThenBy(i => i.Type == main.Type ? 0 : 1).ThenBy(i => i.Id).FirstOrDefault();
    }
}
