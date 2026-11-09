namespace NorthIsland.Server.Features.Mall;

/// <summary>One possible reward of a box.</summary>
/// <param name="ItemId">The item given.</param>
/// <param name="Count">How many of it are given.</param>
/// <param name="Weight">Relative chance of this reward within its table.</param>
public sealed record MallBoxEntry(
    ushort ItemId,
    byte Count,
    int Weight);
