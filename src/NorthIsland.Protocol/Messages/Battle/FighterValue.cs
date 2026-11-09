namespace NorthIsland.Protocol.Messages.Battle;

/// <summary>One attribute change on a target in an action, such as HP lost.</summary>
/// <param name="AttrId">Attribute id, for example <see cref="BattleMessages.AttrHp"/>.</param>
/// <param name="Amount">Size of the change.</param>
/// <param name="Negative">True when the amount is subtracted.</param>
/// <param name="Critical">True when the subtraction is a critical hit; only the 2014 client shows it.</param>
public readonly record struct FighterValue(
    byte AttrId,
    int Amount,
    bool Negative,
    bool Critical = false);
