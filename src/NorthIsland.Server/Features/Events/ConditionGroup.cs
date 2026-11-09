using NorthIsland.GameData.Events;

namespace NorthIsland.Server.Features.Events;

/// <summary>The conditions of one step of an event, with the results that run when they all hold.</summary>
/// <param name="EventIndex">The index of the event the group belongs to.</param>
/// <param name="Conditions">The condition records.</param>
/// <param name="Results">The result records that follow when all conditions hold.</param>
public sealed record ConditionGroup(
    int EventIndex,
    IReadOnlyList<EveScriptRecord> Conditions,
    IReadOnlyList<EveScriptRecord> Results)
{
    /// <summary>A short name for the group in the log: the event index and the step of its first condition.</summary>
    public string Label => $"event {EventIndex} step {Conditions[0].Step}";
}
