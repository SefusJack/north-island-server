using NorthIsland.GameData.Items;

namespace NorthIsland.Server.Features.Items;

/// <summary>The kinds of potion and bottle: which attribute they restore and how much they hold.</summary>
public static class Bottles
{
    /// <summary>The most a bottle pool can hold.</summary>
    public const int PoolMax = 2000000;

    /// <summary>The item ids of the HP bottle and the SP bottle.</summary>
    public const ushort HpBottle = 34298, SpBottle = 34299; // 34298 HP Bottle, 34299 SP Bottle

    /// <summary>The kind of an item as a potion or bottle.</summary>
    /// <param name="item">The item definition.</param>
    /// <returns>1 for an HP potion, 2 for an SP potion, 3 for the HP bottle, 4 for the SP bottle, 0 for any other item.</returns>
    public static byte KindOf(ItemRecord item)
    {
        byte kind = 0;
        if (item.SubType == 151 && item.Words36.Length > 0)
            kind = item.Words36[0] == 25 ? (byte)1 : item.Words36[0] == 26 ? (byte)2 : (byte)0;
        return item.Id switch
        {
            HpBottle => 3, // HP bottle is kind 3
            SpBottle => 4, // SP bottle is kind 4
            _ => kind // any other item keeps the kind found above
        };
    }

    /// <summary>How much a potion restores in all.</summary>
    /// <param name="item">The item definition.</param>
    /// <returns>The capacity, 0 when the item has none.</returns>
    public static int Capacity(ItemRecord item) => item.Ints42.Length > 0 ? Math.Max(0, item.Ints42[0] - 100) : 0;
}
