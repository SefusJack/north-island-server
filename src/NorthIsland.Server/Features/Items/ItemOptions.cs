namespace NorthIsland.Server.Features.Items;

/// <summary>Settings of the items feature.</summary>
public sealed class ItemOptions
{
    /// <summary>The items a new character starts with.</summary>
    public StarterKitOptions StarterKit { get; set; } = new();

    /// <summary>The amounts and times of item effects.</summary>
    public ItemUseOptions ItemUse { get; set; } = new();

    /// <summary>Equipment wear settings.</summary>
    public DurabilityOptions Durability { get; set; } = new();

    /// <summary>Below this level a player is asked to confirm before dropping an item; 0 never asks.</summary>
    public int DropConfirmBelowLevel { get; set; } = 10;

    /// <summary>The item ids the bag sort puts first, in this order; the other items follow as usual.</summary>
    public List<ushort> BagSortFirst { get; set; } = [];
}
