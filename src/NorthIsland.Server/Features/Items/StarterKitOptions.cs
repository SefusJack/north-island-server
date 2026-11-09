namespace NorthIsland.Server.Features.Items;

/// <summary>The items a new character starts with.</summary>
public sealed class StarterKitOptions
{
    /// <summary>Whether new characters get the starter kit.</summary>
    public bool Enabled { get; set; } = true;

    /// <summary>Item ids put on after the role's default outfit.</summary>
    public List<ushort> Equip { get; set; } = [];

    /// <summary>Item ids put in the bag.</summary>
    public List<ushort> Bag { get; set; } = [];
}
