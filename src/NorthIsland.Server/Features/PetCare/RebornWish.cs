namespace NorthIsland.Server.Features.PetCare;

/// <summary>The fixed numbers of the reborn wish: the star cost by grow key and the career marks that halve it.</summary>
public static class RebornWish
{
    /// <summary>The star cost of a wish, by the pet's grow key.</summary>
    public static readonly IReadOnlyDictionary<byte, byte> StarCost = new Dictionary<byte, byte>
    {
        [1] = 2, [2] = 4, [3] = 6, [4] = 5, [5] = 8, [6] = 6, [8] = 4, [9] = 6, [10] = 7, [11] = 5, [12] = 10, [16] = 7, [17] = 8, [22] = 7,
    };

    /// <summary>For some pets, the career mark whose flag halves the star cost.</summary>
    public static readonly IReadOnlyDictionary<ushort, ushort> CareerMark = new Dictionary<ushort, ushort>
    {
        [14156] = 50363, // 14156 Xaolan, 50363 Xaolan rebirth mark
        [14081] = 50365, // 14081 Niss, 50365 Niss rebirth mark
        [14162] = 50367, // 14162 Roca, 50367 Roca rebirth mark
        [14175] = 50369, // 14175 Clive, 50369 Clive rebirth mark
        [14615] = 50371, // 14615 Qlaya, 50371 Qlaya rebirth mark
        [12068] = 50373, // 12068 Magellan, 50373 Magellan rebirth mark
        [14609] = 50377, // 14609 Shasha, 50377 Shasha rebirth mark
        [14230] = 50383, // 14230 Elin, 50383 Elin's Rebirth
        [15689] = 50385, // 15689 Hayate, 50385 Hayate's Rebirth
        [14548] = 50375, // 14548 Kanako, 50375 Kanako rebirth mark
        [12095] = 50387, // 12095 Frederico, 50387 Fred's Rebirth
        [15657] = 50389, // 15657 Victoria, 50389 Victoria's Rebirth
        [14274] = 50391, // 14274 Sam, 50391 Sam's Rebirth
    };

    /// <summary>Works out the star cost of wishing for a pet.</summary>
    /// <param name="growKey">The pet's grow key.</param>
    /// <param name="npcId">The pet's NPC id.</param>
    /// <param name="hasFlag">Tells whether the player has a mark flag.</param>
    /// <returns>The cost in stars, halved and rounded up with the pet's career mark; 0 when the grow key has no cost.</returns>
    public static int Cost(byte growKey, ushort npcId, Func<ushort, bool> hasFlag)
    {
        if (!StarCost.TryGetValue(growKey, out var cost))
            return 0;
        return CareerMark.TryGetValue(npcId, out var mark) && hasFlag(mark) ? (cost + 1) / 2 : cost;
    }
}
