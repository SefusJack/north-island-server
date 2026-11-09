namespace NorthIsland.Server.Features.Mall;

/// <summary>Settings and catalog of the item mall.</summary>
public sealed class ItemMallOptions
{
    /// <summary>Whether the mall is offered; when false no catalog is sent.</summary>
    public bool Enabled { get; set; } = true;

    /// <summary>Whether the mall's game tab is opened for the client on login.</summary>
    public bool GameTab { get; set; } = true;

    /// <summary>Mall points an account receives once, on its first login.</summary>
    public int StartingPoints { get; set; } = 1000;

    /// <summary>The configured mall rows; when empty, <see cref="DefaultItems"/> is used.</summary>
    public List<MallItem> Items { get; set; } = [];

    /// <summary>Share, in percent, of the points spent in the mall that is credited as bonus points.</summary>
    public int BonusPercent { get; set; }

    /// <summary>The rows of the second list, bought with bonus points.</summary>
    public List<MallItem> BonusItems { get; set; } = [];

    /// <summary>Number of rotation groups; 0 turns rotation off.</summary>
    public int RotationGroups { get; set; }

    /// <summary>Days each rotation group stays on sale.</summary>
    public int RotationDays { get; set; } = 7;

    /// <summary>The configured rows, or the built-in defaults when none are configured.</summary>
    public IReadOnlyList<MallItem> Catalog => Items.Count > 0 ? Items : DefaultItems;

    /// <summary>Finds the rotation group that is on sale at a time.</summary>
    /// <param name="utc">The time to look up (UTC).</param>
    /// <returns>The group number from 1, or 0 when rotation is off.</returns>
    public int RotationGroupAt(DateTime utc)
    {
        if (RotationGroups <= 0)
            return 0;
        var turn = (long)Math.Floor((utc - RotationStart).TotalDays / Math.Max(1, RotationDays));
        return (int)(((turn % RotationGroups) + RotationGroups) % RotationGroups) + 1;
    }

    /// <summary>Start of the first rotation turn.</summary>
    private static readonly DateTime RotationStart = new(2026, 1, 5, 0, 0, 0, DateTimeKind.Utc);

    /// <summary>Built-in rows used when the configuration lists none.</summary>
    public static readonly IReadOnlyList<MallItem> DefaultItems =
        [
            new() { Item = 34041, Category = 2, Price = 30, Hot = 1 }, // Hair Dye
            new() { Item = 34042, Category = 2, Price = 30 }, // Color Mask
            new() { Item = 34040, Category = 2, Price = 30 }, // Strange Eye-drops
            new() { Item = 34228, Category = 2, Price = 30, Hot = 1 }, // Clothes Dye
            new() { Item = 34023, Category = 3, Price = 150 }, // Box
            new() { Item = 34029, Category = 3, Price = 15 }, // Protective EXP Bolus
            new() { Item = 34029, Category = 4, Pack = 10, Price = 130, Discount = 90 }, // Protective EXP Bolus
            new() { Item = 61000, Category = 1, Price = 20 }, // Clear Devil's Spar
            new() { Item = 61001, Category = 1, Price = 40 }, // Amber Devil's Spar
            new() { Item = 61002, Category = 1, Price = 60 }, // Cyan Devil's Spar
            new() { Item = 62000, Category = 1, Price = 80 }, // 8% Bomb Diamond
        ];
}
