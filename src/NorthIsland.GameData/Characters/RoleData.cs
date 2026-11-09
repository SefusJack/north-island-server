using System.Collections.Immutable;

namespace NorthIsland.GameData.Characters;

/// <summary>Static tables for the playable roles: body type and head mapping, role skills, creation-wizard equipment and stat bonuses.</summary>
public static class RoleData
{
    /// <summary>Maps a creation-wizard (body type, head) pair to its role index.</summary>
    /// <param name="bodyType">The body type chosen in the wizard.</param>
    /// <param name="head">The head chosen in the wizard.</param>
    /// <returns>The role index (1..15), or 0 when the pair is not a creatable character.</returns>
    public static byte RoleIndex(byte bodyType, byte head) => (bodyType, head) switch
    {
        (1, 0) => 3, // body type/head 1, 0 is role 3
        (2, 0) => 1, // body type/head 2, 0 is role 1
        (2, 1) => 2, // body type/head 2, 1 is role 2
        (3, 0) => 6, // body type/head 3, 0 is role 6
        (3, 1) => 7, // body type/head 3, 1 is role 7
        (3, 2) => 12, // body type/head 3, 2 is role 12
        (3, 3) => 14, // body type/head 3, 3 is role 14
        (4, 0) => 10, // body type/head 4, 0 is role 10
        (4, 1) => 5, // body type/head 4, 1 is role 5
        (4, 2) => 4, // body type/head 4, 2 is role 4
        (4, 3) => 13, // body type/head 4, 3 is role 13
        (4, 4) => 8, // body type/head 4, 4 is role 8
        (4, 5) => 9, // body type/head 4, 5 is role 9
        (4, 6) => 11, // body type/head 4, 6 is role 11
        (4, 7) => 15, // body type/head 4, 7 is role 15
        _ => 0, // not a creatable pair: no role
    };

    /// <summary>Maps a role index back to its (body type, head) pair.</summary>
    /// <param name="role">The role index.</param>
    /// <returns>The pair, or null for a role outside 1..13.</returns>
    public static (byte BodyType, byte Head)? SplitRole(byte role) => role switch
    {
        1 => (2, 0), // role 1: body type 2, head 0
        2 => (2, 1), // role 2: body type 2, head 1
        3 => (1, 0), // role 3: body type 1, head 0
        4 => (4, 2), // role 4: body type 4, head 2
        5 => (4, 1), // role 5: body type 4, head 1
        6 => (3, 0), // role 6: body type 3, head 0
        7 => (3, 1), // role 7: body type 3, head 1
        8 => (4, 4), // role 8: body type 4, head 4
        9 => (4, 5), // role 9: body type 4, head 5
        10 => (4, 0), // role 10: body type 4, head 0
        11 => (4, 6), // role 11: body type 4, head 6
        12 => (3, 2), // role 12: body type 3, head 2
        13 => (4, 3), // role 13: body type 4, head 3
        _ => null, // any other role: no pair
    };

    /// <summary>
    /// Per role (row index), the item ids the creation wizard puts on the preview character; column 0 is unused, columns 1..6 are equip slots.
    /// </summary>
    private static readonly ImmutableArray<ImmutableArray<ushort>> DefaultEquipmentTable =
        [
            [0, 0, 0, 0, 0, 0, 0],
            [0, 22003, 21002, 0, 0, 24002, 0], // 22003 Star Headdress, 21002 Small Dress, 24002 Moppet's Shoes
            [0, 22001, 21003, 0, 0, 24003, 0], // 22001 Wing Headdress, 21003 Harness pants, 24003 Pink Shoes
            [0, 22004, 21001, 0, 0, 24001, 0], // 22004 Lovely Headdress, 21001 Boy's Suit, 24001 Children's Shoes
            [0, 0, 21008, 0, 0, 24008, 0], // 21008 Uniform, 24008 Student's Shoes
            [0, 0, 21007, 0, 23002, 24007, 0], // 21007 Racing Suit, 23002 Competition Gloves, 24007 Racing Shoes
            [0, 0, 21004, 0, 0, 24004, 0], // 21004 Dust Coat, 24004 Gentleman's Shoes
            [0, 0, 21005, 0, 0, 24005, 0], // 21005 Jeans, 24005 Leisure Shoes
            [0, 22002, 21010, 10003, 0, 24010, 0], // 22002 Flower Headdress, 21010 Violinists Suit, 10003 Violin, 24010 High-heeled Shoes
            [0, 0, 21013, 0, 0, 24013, 0], // 21013 Kimono, 24013 Wooden Shoes
            [0, 22005, 21006, 0, 23001, 24006, 0], // 22005 Sailor's Hat, 21006 Sailor's Suit, 23001 Gloves, 24006 White Boots
            [0, 22006, 21011, 10004, 0, 24011, 0], // 22006 Nurse's Hat, 21011 Nurse's Clothes, 10004 Syringe, 24011 White Leather Shoes
            [0, 0, 21012, 0, 0, 24012, 0], // 21012 Bartender's Uniform, 24012 Great Pumps
            [0, 22007, 21009, 10002, 0, 24009, 0], // 22007 Round Headdress, 21009 Restaurant Uniform, 10002 plate, 24009 Canvas
            [0, 22009, 21014, 18002, 0, 24014, 0], // 22009 Headscarf, 21014 Craftsman's Clothes, 18002 Craftsman's Pestle, 24014 Craftsman Shoes
            [0, 22008, 21015, 0, 0, 24015, 0], // 22008 Band, 21015 Uniform, 24015 Leather Shoes
        ];

    /// <summary>Skill id of the "Newbie's Stunt" skill, the learned skill behind every character's role skill.</summary>
    public const ushort NewbieSkill = 15003; // Newbie's Stunt

    /// <summary>Returns the skill id of a role's own skill.</summary>
    /// <param name="role">The role index.</param>
    /// <returns>The skill id, or 0 for an unknown role.</returns>
    public static ushort RoleSkill(byte role) => role switch
    {
        1 => 15041, // Love Wish
        2 => 12036, // Leap
        3 => 11075, // Summon Dogs Groups
        4 => 15040, // Palm
        5 => 12053, // Gallop
        6 => 15038, // Overarm Stumble
        7 => 11076, // Combo x3 Attack
        8 => 12051, // Note
        9 => 12049, // Fire Dance
        10 => 11078, // Bomber Attack
        11 => 11077, // Cure 2 Players
        12 => 15039, // Wine Flame
        13 => 15060, // Throw Dish
        14 => 11182, // Ghost Hammer
        15 => 11183, // Deacon Attack
        _ => 0, // unknown role: no skill
    };

    /// <summary>Tells whether a skill id is the own skill of one of the 15 roles.</summary>
    /// <param name="skillId">The skill id to test.</param>
    /// <returns>True when it is a role skill id.</returns>
    public static bool IsRoleSkill(ushort skillId) => skillId != 0 && Enumerable.Range(1, 15).Any(r => RoleSkill((byte)r) == skillId);

    /// <summary>Returns the clothes item a role must wear (equip slot 2) to use its role skill.</summary>
    /// <param name="role">The role index.</param>
    /// <returns>The item id, or 0 for an unknown role.</returns>
    public static ushort RoleSkillClothes(byte role) => role < DefaultEquipmentTable.Length ? DefaultEquipmentTable[role][2] : (ushort)0;

    /// <summary>Per role (row index), the stat bonuses of the creation wizard; column 0 is unused, columns 1..5 are STR, CON, INT, WIS, AGI.</summary>
    private static readonly ImmutableArray<ImmutableArray<ushort>> StatBonusTable =
        [
            [0, 0, 0, 0, 0, 0],
            [0, 0, 0, 1, 1, 1],
            [0, 0, 0, 1, 0, 2],
            [0, 0, 0, 2, 0, 1],
            [0, 0, 0, 3, 0, 0],
            [0, 2, 0, 0, 0, 1],
            [0, 1, 2, 0, 0, 0],
            [0, 2, 1, 0, 0, 0],
            [0, 0, 0, 1, 2, 0],
            [0, 0, 0, 2, 1, 0],
            [0, 1, 2, 0, 0, 0],
            [0, 0, 0, 2, 0, 1],
            [0, 0, 0, 0, 2, 1],
            [0, 0, 0, 0, 0, 0],
            [0, 1, 1, 1, 0, 0],
            [0, 0, 0, 1, 1, 1],
        ];

    /// <summary>Returns the item ids a new character of the role starts dressed in.</summary>
    /// <param name="role">The role index.</param>
    /// <returns>The non-zero item ids of equip columns 1..6, empty for an unknown role.</returns>
    public static IReadOnlyList<ushort> DefaultEquipment(byte role) =>
        role < DefaultEquipmentTable.Length ? [.. DefaultEquipmentTable[role].Skip(1).Take(6).Where(id => id != 0)] : [];

    /// <summary>Returns the role's bonus per stat.</summary>
    /// <param name="role">The role index.</param>
    /// <returns>Five values in the order STR, CON, INT, WIS, AGI; all zero for an unknown role.</returns>
    public static IReadOnlyList<ushort> StatBonus(byte role) =>
        role < StatBonusTable.Length ? [.. StatBonusTable[role].Skip(1).Take(5)] : [0, 0, 0, 0, 0];

    /// <summary>Computes a new character's starting attributes from the creation points and the role bonus.</summary>
    /// <param name="creationStats">The five stat bytes of packet <c>9/1</c>, in the order the wizard sends them.</param>
    /// <param name="role">The role index whose bonus is added.</param>
    /// <returns>The five attributes.</returns>
    public static StartingAttributes StartingAttributes(IReadOnlyList<byte> creationStats, byte role)
    {
        ArgumentNullException.ThrowIfNull(creationStats);
        if (creationStats.Count < 5)
            throw new ArgumentException("9/1 carries five stat bytes.", nameof(creationStats));
        var bonus = StatBonus(role);
        return new StartingAttributes(
            Strength: (ushort)(creationStats[0] + bonus[0]),
            Constitution: (ushort)(creationStats[4] + bonus[1]),
            Intelligence: (ushort)(creationStats[3] + bonus[2]),
            Wisdom: (ushort)(creationStats[2] + bonus[3]),
            Agility: (ushort)(creationStats[1] + bonus[4]));
    }
}
