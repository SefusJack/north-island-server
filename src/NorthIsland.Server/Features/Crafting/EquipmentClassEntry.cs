namespace NorthIsland.Server.Features.Crafting;

/// <summary>A row of the equipment class table: the material classes of one piece of equipment.</summary>
/// <param name="Name">The item name the row was made for.</param>
/// <param name="Words">The class numbers of the item.</param>
public sealed record EquipmentClassEntry(
    string Name,
    ushort[] Words);
