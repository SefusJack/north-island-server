namespace NorthIsland.GameData.Events;

/// <summary>One condition of an event with the results that run when it holds.</summary>
/// <param name="Record">The condition record.</param>
/// <param name="Results">The result records that belong to the condition.</param>
public sealed record EveCondition(
    EveScriptRecord Record,
    IReadOnlyList<EveScriptRecord> Results);
