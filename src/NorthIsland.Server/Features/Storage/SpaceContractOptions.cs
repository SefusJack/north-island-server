namespace NorthIsland.Server.Features.Storage;

/// <summary>The tunable amounts of the Space Contract.</summary>
public sealed class SpaceContractOptions
{
    /// <summary>
    /// Whether every part of the contract needs a running contract that includes it; when false only the stock keeper and garage parts do.
    /// </summary>
    public bool Required { get; set; }

    /// <summary>The days one use of a contract item adds to the running time.</summary>
    public int DaysPerUse { get; set; } = 30;

    /// <summary>The most days a contract may run in total; a use that would pass it is refused.</summary>
    public int MaxDays { get; set; } = 360;

    /// <summary>The pet storage slots of the hotel that need no hotel part.</summary>
    public int HotelFreeSlots { get; set; } = 8;

    /// <summary>The house room size (1 to 3) while a tent part runs.</summary>
    public byte TentSize { get; set; } = 2;
}
