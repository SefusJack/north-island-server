namespace NorthIsland.Server.Features.Houses;

/// <summary>The owner of a tent or house, whether online or not.</summary>
/// <param name="Id">The owner's account login id.</param>
/// <param name="Character">The owner's character that holds the tent and the furniture.</param>
/// <param name="Session">The owner's session; null when the owner is not online.</param>
public sealed record TentOwner(
    int Id,
    Character Character,
    GameSession? Session);
