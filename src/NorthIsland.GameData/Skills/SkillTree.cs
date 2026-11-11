using System.Collections.Immutable;

namespace NorthIsland.GameData.Skills;

/// <summary>The element skill trees: for each element 1-4 the skills of its cells and the stats they need.</summary>
public static class SkillTree
{
    /// <summary>The number of cells of one element's tree.</summary>
    public const int CellsPerElement = 18;

    /// <summary>The tree table as u16 words, six per cell.</summary>
    private static readonly ImmutableArray<ushort> Words = Decode(
        [
            15085, 0, 0, // 15085 Rock Attack
            1059593, 0, 0, // 11017 Earth Attack
            1846095, 0, 0, // 11087 Rupture Attack
            2898667, 0, 2, // 15083 Boulder Attack
            3947209, 5, 0, // 15049 Jump Attack
            5258026, 0, 262144, // 15146 Scurry Attack
            12006, 0, 0, // 12006 Rock Elf Attack
            15056, 1048576, 0, // 15056 Rockfall Attack
            11019, 1835008, 0, // 11019 Rock Blast Attack
            11031, 3014656, 0, // 11031 Rock Ball Attack
            15086, 4390912, 0, // 15086 Stress Attack
            11107, 5898240, 3, // 11107 Megalith Attack
            11057, 0, 0, // 11057 Shield Defence
            12043, 0, 11, // 12043 Wake Spell
            12048, 0, 18, // 12048 Ice-out
            11055, 0, 29, // 11055 Stone Wall
            15070, 4, 38, // 15070 Tree Bind
            15035, 262144, 48, // 15035 Terrafication
            15091, 0, 0, // 15091 Ice Attack
            862969, 0, 0, // 11001 Icicle Attack
            1256228, 65536, 0, // 11044 Water Attack
            1784491, 131072, 0, // 15019 Turning Ice Attack
            2305767, 196608, 0, // 12007 Frost Attack
            3816244, 262144, 0, // 15156 Eddy Attack
            15097, 0, 0, // 15097 Water Pole Attack
            11040, 1048576, 0, // 11040 Chill Attack
            11110, 1835008, 0, // 11110 Ice Ball Attack
            11113, 3014656, 0, // 11113 Ice Elf Attack
            11024, 4259840, 2, // 11024 Thunder Attack
            15158, 5570560, 6, // 15158 Water Thunder Attack
            15100, 0, 0, // 15100 Detoxifcation
            11042, 0, 13, // 11042 Cure Spell
            15075, 0, 19, // 15075 Magic Recovery
            11080, 0, 25, // 11080 Water Shield
            11051, 65536, 30, // 11051 Freezing Spell
            11043, 0, 36, // 11043 Recovery
            11016, 0, 0, // 11016 Flame Attack
            1063677, 0, 0, // 15101 Sword Awn Attack
            1846122, 0, 0, // 11114 Turning Fire Attack
            3026695, 0, 0, // 12039 Fire Wave Attack
            4274942, 0, 2, // 15102 Five Star Attack
            5585604, 0, 262144, // 15044 Hagendis Attack
            11166, 0, 0, // 11166 Blast Attack
            11005, 983040, 2, // 11005 Fire Blast Attack
            11034, 1769472, 0, // 11034 Fire Ball Attack
            15109, 2949120, 0, // 15109 Blaze Attack
            15111, 4325376, 0, // 15111 Fire Combo Attack
            11035, 5636096, 6, // 11035 Fire Stone Attack
            11056, 0, 0, // 11056 Slowdown
            11002, 0, 11, // 11002 Poison Spell
            11072, 0, 18, // 11072 Fiery Attack
            12045, 0, 29, // 12045 Free From Coma
            11003, 0, 42, // 11003 Mess Spell
            15171, 65536, 55, // 15171 Fire Attack
            11007, 0, 0, // 11007 Wind Attack
            342759, 0, 655360, // 15079 Air Attack
            670477, 0, 1114112, // 15117 Cyclone Attack
            15002, 0, 2293760, // 15002 Instant Attack
            1321766, 0, 3014656, // 11046 Wind Stress Attack
            2308874, 0, 3604480, // 15114 Dead Wind Attack
            30002, 0, 0, // 30002 Wind Blade Attack
            11015, 983040, 0, // 11015 Wind Cut Attack
            15123, 1507328, 5, // 15123 Whirlwind Attack
            15125, 2686976, 0, // 15125 Space Attack
            15048, 4063232, 0, // 15048 Sky Attack
            15161, 5636096, 0, // 15161 Air Stress Attack
            11052, 0, 0, // 11052 Speedup
            11073, 0, 11, // 11073 Shield Smash
            12046, 0, 18, // 12046 Unload Wall
            15032, 262144, 25, // 15032 Cord Spell
            15036, 0, 38, // 15036 Unstun
            11026, 458752, 49, // 11026 Coma Spell
        ]);

    /// <summary>
    /// The hidden skills of each element: not in any tree cell, learned and forgotten by their stat requirements like the non-root cells.
    /// </summary>
    private static readonly ImmutableArray<ImmutableArray<SkillTreeEntry>> HiddenSkills =
    [
        [ // 1 Earth
            new(12018, 0, 0, 114, 9, 0), // 12018 Acrolith Attack
            new(15066, 1, 12, 58, 58, 30), // 15066 Summon Mosquito
        ],
        [ // 2 Water
            new(12042, 0, 0, 1, 42, 0), // 12042 Free From mess
            new(12041, 0, 0, 2, 47, 0), // 12041 Sleep
            new(11053, 0, 0, 1, 53, 0), // 11053 Revival
            new(15042, 0, 0, 3, 58, 0), // 15042 Recover Normal
        ],
        [], // 3 Fire
        [ // 4 Wind
            new(15128, 0, 0, 0, 0, 85), // 15128 Shadow Attack
            new(15120, 50, 0, 0, 0, 103), // 15120 Afterimage Attack
            new(21401, 65, 0, 0, 0, 124), // 21401 Storm Attack
        ],
    ];

    /// <summary>The hidden skills of one element, with the stats they need.</summary>
    /// <param name="element">The element 1-4.</param>
    /// <returns>The hidden skills; empty for any other element.</returns>
    public static IReadOnlyList<SkillTreeEntry> Hidden(int element) => element is >= 1 and <= 4 ? HiddenSkills[element - 1] : [];

    /// <summary>The cells of one element's tree.</summary>
    /// <param name="element">The element 1-4.</param>
    /// <returns>The cells in order; empty for any other element.</returns>
    public static IReadOnlyList<SkillTreeEntry> For(int element)
    {
        if (element is < 1 or > 4)
            return [];
        var cells = new SkillTreeEntry[CellsPerElement];
        for (var i = 0; i < CellsPerElement; i++)
        {
            var w = ((element - 1) * CellsPerElement + i) * 6;
            cells[i] = new SkillTreeEntry(Words[w],
                Words[w + 1], // 1 Strength
                Words[w + 2], // 2 Constitution
                Words[w + 3], // 3 Intelligence
                Words[w + 4], // 4 Wisdom
                Words[w + 5]); // 5 Agility
        }
        return cells;
    }

    /// <summary>Splits each int into its low and high u16.</summary>
    /// <param name="ints">The table as ints.</param>
    /// <returns>The u16 words.</returns>
    private static ImmutableArray<ushort> Decode(int[] ints)
    {
        var words = new ushort[ints.Length * 2];
        for (var i = 0; i < ints.Length; i++)
        {
            words[2 * i] = (ushort)(ints[i] & 0xFFFF);
            words[2 * i + 1] = (ushort)((ints[i] >> 16) & 0xFFFF);
        }
        return [.. words];
    }
}
