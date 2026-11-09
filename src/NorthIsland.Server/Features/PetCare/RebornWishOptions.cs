namespace NorthIsland.Server.Features.PetCare;

/// <summary>Settings of the reborn wish.</summary>
public sealed class RebornWishOptions
{
    /// <summary>Whether the wish is on.</summary>
    public bool Enabled { get; set; } = true;

    /// <summary>The item id of the star that pays for a wish.</summary>
    public ushort StarItem { get; set; } = 30025; // Star

    /// <summary>The NPC ids of the pets on offer; empty means every pet whose grow key has a star cost.</summary>
    public List<ushort> Pets { get; set; } = [];
}
