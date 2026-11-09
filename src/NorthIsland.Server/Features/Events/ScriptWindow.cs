using NorthIsland.GameData.Events;

namespace NorthIsland.Server.Features.Events;

/// <summary>The span of the dates in the date conditions of some map events.</summary>
/// <param name="First">The earliest date.</param>
/// <param name="Last">The latest date.</param>
public sealed record ScriptWindow(
    DateTime First,
    DateTime Last)
{
    /// <summary>Finds the span of the date conditions (kind 18) of the listed events of a scene, lead events included.</summary>
    /// <param name="scene">The scene, or null when the map is unknown.</param>
    /// <param name="events">The event indexes to look at.</param>
    /// <returns>The span; null when the scene is unknown or none of the events has a date condition.</returns>
    public static ScriptWindow? Of(EveScene? scene, IReadOnlyList<int> events)
    {
        if (scene is null)
            return null;
        var dates = scene.Events.Concat(scene.LeadEvents).Where(e => events.Contains(e.Index))
            .SelectMany(e => e.Conditions).Select(c => c.Record).Where(r => r.Kind == 18)
            .Select(ConditionEvaluator.EventDate).OfType<DateTime>().ToList();
        return dates.Count == 0 ? null : new ScriptWindow(dates.Min(), dates.Max());
    }
}
