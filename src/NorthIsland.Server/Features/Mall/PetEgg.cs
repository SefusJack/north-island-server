namespace NorthIsland.Server.Features.Mall;

/// <summary>
/// The Pet Egg: a pet joins the team at random. A 2009 forum post says the 7 Colored Egg gave the same pets as the normal pet eggs
/// (rhino, vale butterfly, blue horse, lion), so the pool is the 7 Colors Egg's list without the top pets IGG named for it (Sano,
/// Houyi, Wuwan, the Yangs' Saga, the Water Monster, the Unicorn and the Holy Pegasus), with the same weights.
/// </summary>
public static class PetEgg
{
    /// <summary>Item id of the Pet Egg.</summary>
    public const ushort ItemId = 34025;

    /// <summary>The pets an egg can give (Npc.dat id) and their weights.</summary>
    public static readonly IReadOnlyList<(ushort NpcId, int Weight)> Pool =
    [
        (17214, 40), // Aged Turtle
        (17215, 40), // Dongfeng Crane
        (17216, 100), // Rhino
        (17217, 40), // Zebra
        (17218, 30), // Blue Horse Kiny
        (17219, 120), // Lion
        (17220, 10), // Devil Naji
        (17221, 20), // Deadly Flower
        (17222, 20), // Pink Ginseng
        (17223, 120), // Vale Butterfly
        (17224, 120), // Gold Horn Buffalo
        (17225, 10), // Bigfoot Harley
        (30099, 100), // Globefish
        (30100, 100), // Camel
        (30101, 40), // Pterosaur
        (30102, 40), // Octopus
        (30103, 15), // Sea Lion
        (30104, 10), // Skeleton Soldier
        (30105, 10), // Gold Griffin
    ];

    /// <summary>Picks a pet by weight.</summary>
    /// <param name="roll">Random source: <c>roll(n)</c> returns an integer from 0 to n - 1.</param>
    /// <returns>The Npc.dat id of the pet.</returns>
    public static ushort Roll(Func<int, int> roll)
    {
        var pick = roll(Pool.Sum(p => p.Weight));
        foreach (var (npcId, weight) in Pool)
        {
            if (pick < weight)
                return npcId;
            pick -= weight;
        }
        return Pool[^1].NpcId;
    }
}
