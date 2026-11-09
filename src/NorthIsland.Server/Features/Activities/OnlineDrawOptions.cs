namespace NorthIsland.Server.Features.Activities;

/// <summary>Configuration of the online draw.</summary>
public sealed class OnlineDrawOptions
{
    /// <summary>Switches the online draw on or off.</summary>
    public bool Enabled { get; set; } = true;

    /// <summary>The hour (0 to 23) at which the daily draws start again.</summary>
    public int ResetHour { get; set; } = 2;

    /// <summary>The configured prizes; when empty <see cref="WikiPrizes"/> are used.</summary>
    public List<OnlineDrawPrize> Prizes { get; set; } = [];

    /// <summary>The default prizes.</summary>
    public static IReadOnlyList<OnlineDrawPrize> WikiPrizes { get; } =
        [
            new() { Item = 35116, Weight = 10 }, // HP Restore Pack I
            new() { Item = 35117, Weight = 10 }, // SP Restore Pack I
            new() { Item = 34329, Weight = 10 }, // Spice Pack
            new() { Item = 34328, Weight = 10 }, // Magic Perfume
            new() { Item = 34326, Weight = 10 }, // Mustard Pack
            new() { Item = 34327, Weight = 10 }, // Body Lotion
            new() { Item = 34124, Weight = 10 }, // Fast Growth Statue
            new() { Item = 34147, Weight = 10 }, // 2.5 Holy EXP Potion
            new() { Item = 34148, Weight = 10 }, // Space Agreement
            new() { Item = 34089, Weight = 1, Top = true }, // Forgotten Scroll
            new() { Item = 34139, Weight = 1, Top = true }, // Pet Return Scroll
            new() { Item = 34136, Weight = 1, Top = true }, // 1% EXP Capsule
        ];
}
