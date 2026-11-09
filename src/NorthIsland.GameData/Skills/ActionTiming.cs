namespace NorthIsland.GameData.Skills;

/// <summary>The estimated time in ms the original client needs to play one skill action, split into its parts.</summary>
/// <param name="AttackerMs">Until the attacker has finished its motion and is back at its cell.</param>
/// <param name="HitMs">From the start of the action until the targets are hit.</param>
/// <param name="ReactionMs">Until the targets' reaction motion is over; 0 when the targets play none.</param>
/// <param name="NumbersMs">Until the floating damage numbers are gone; 0 when none are shown.</param>
/// <param name="TotalMs">The longest of the attacker, reaction and number times: when the action is over.</param>
public readonly record struct ActionTiming(
    int AttackerMs,
    int HitMs,
    int ReactionMs,
    int NumbersMs,
    int TotalMs);
