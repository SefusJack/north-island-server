namespace NorthIsland.Server.Features.Travel;

/// <summary>One place the magic travel items can take a player to.</summary>
/// <param name="Name">The name of the place.</param>
/// <param name="Map">The map id.</param>
/// <param name="X">The x coordinate.</param>
/// <param name="Y">The y coordinate.</param>
public sealed record MagicMapPlace(
    string Name,
    ushort Map,
    ushort X,
    ushort Y);
