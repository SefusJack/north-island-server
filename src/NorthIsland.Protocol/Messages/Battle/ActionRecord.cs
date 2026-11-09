namespace NorthIsland.Protocol.Messages.Battle;

/// <summary>One attacker's entry in a 50/1 packet: who acts, with which skill, and what happens to each target.</summary>
/// <param name="Row">Battle row of the attacker.</param>
/// <param name="Col">Battle column of the attacker.</param>
/// <param name="SkillId">Id of the skill or attack used.</param>
/// <param name="Targets">What the action does to each target.</param>
public sealed record ActionRecord(
    byte Row,
    byte Col,
    ushort SkillId,
    IReadOnlyList<ActionTarget> Targets);
