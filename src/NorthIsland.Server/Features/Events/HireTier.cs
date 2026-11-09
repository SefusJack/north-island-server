namespace NorthIsland.Server.Features.Events;

/// <summary>The price and conditions of one hire tier of a team NPC.</summary>
/// <param name="Points">The mall points the account is charged.</param>
/// <param name="MinLevel">The level the character needs.</param>
/// <param name="RefusalTalk">The dialog text the NPC says when the hire is refused; 0 for none.</param>
public readonly record struct HireTier(
    int Points,
    int MinLevel,
    int RefusalTalk);
