namespace NorthIsland.Protocol.Messages.Battle;

/// <summary>What one action does to one target fighter.</summary>
/// <param name="Row">Battle row of the target.</param>
/// <param name="Col">Battle column of the target.</param>
/// <param name="Hit">True when the action connected.</param>
/// <param name="Guard">Defence state byte for the target; the server sends 1 when the target was defending, 4 for a failed capture and 0 otherwise.</param>
/// <param name="Values">The attribute changes shown on the target.</param>
public sealed record ActionTarget(
    byte Row,
    byte Col,
    bool Hit,
    byte Guard,
    IReadOnlyList<FighterValue> Values);
