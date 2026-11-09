namespace NorthIsland.GameData.Items;

/// <summary>One manufacture recipe from Compound2.dat: what is made, at which furniture facility and from which materials.</summary>
/// <param name="Index">The 1-based record number in the file.</param>
/// <param name="Product">Item id of the product.</param>
/// <param name="Blueprint">Item id of the blueprint the player must have for the recipe.</param>
/// <param name="Category">Category byte of the recipe, as stored in the file.</param>
/// <param name="Facility">Item id of the furniture facility that makes it; 0 for product 39123.</param>
/// <param name="PerBatch">Number of products one batch yields.</param>
/// <param name="Materials">Item id and count of each material, up to five.</param>
/// <param name="Minutes">Production time of one batch in minutes.</param>
public sealed record CompoundRecipe(
    int Index,
    ushort Product,
    ushort Blueprint,
    byte Category,
    ushort Facility,
    byte PerBatch,
    IReadOnlyList<(ushort Item, byte Count)> Materials,
    ushort Minutes);
