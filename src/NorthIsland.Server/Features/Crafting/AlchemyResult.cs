using NorthIsland.GameData.Items;

namespace NorthIsland.Server.Features.Crafting;

/// <summary>The rolled result of a compound.</summary>
/// <param name="Outcome">Whether an item was made, and how a failure treats the materials.</param>
/// <param name="Product">The item made or put in place of the materials, null when nothing is given.</param>
/// <param name="TargetRank">The highest shop rank the roll reached.</param>
/// <param name="MainClass">The material class the compound followed, which also decides the byproduct of a failure.</param>
public sealed record AlchemyResult(
    AlchemyOutcome Outcome,
    ItemRecord? Product,
    int TargetRank,
    ushort MainClass = 0);
