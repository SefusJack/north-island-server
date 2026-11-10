using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using NorthIsland.GameData.Events;
using NorthIsland.Protocol.Messages.Events;

namespace NorthIsland.Server.Features.Events;

public sealed partial class EventEngine
{
    /// <summary>
    /// Picks a random fight for a field encounter from the field-monster events of the monsters met, and sends the battle to the client.
    /// </summary>
    /// <param name="session">The player's session.</param>
    /// <param name="monsters">The monsters met on the map.</param>
    /// <param name="rng">The random source that picks among the battles found.</param>
    /// <returns>True when a battle was chosen; false when none of the monsters has a usable battle.</returns>
    public bool StartField(GameSession session, IReadOnlyList<EveNpc> monsters, Random rng)
    {
        var character = session.ActiveCharacter!;
        if (maps.Scene(character.MapId) is not { } scene)
            return false;
        var run = NewRun(session, character, TriggerArea, 0, FieldSource);
        List<ushort> battles = [];
        foreach (var monster in monsters)
        {
            foreach (var ev in scene.EventsOf(monster).Where(e => e.IsFieldMonster))
            {
                foreach (var group in ConditionEvaluator.Groups(ev))
                {
                    if (!group.Results.Any(r => r.Kind == 4 && r.SubKind == 1) || !ConditionEvaluator.Holds(group, character, run, NullLogger.Instance))
                        continue;
                    battles.AddRange(group.Results.Where(r => r.Kind == 4 && r.SubKind == 1)
                        .Select(r => r.Arg1)
                        .Where(index => scene.Battles.FirstOrDefault(b => b.Index == index) is { } b && b.Left.Any(m => npcs[(ushort)m.Id] is not null)));
                }
            }
        }
        if (battles.Count == 0)
            return false;
        var chosen = battles[rng.Next(battles.Count)];
        logger.LogInformation("[{Session}] {Name} meets a random fight on map {Map}: battle {Battle}", session.Id, character.Name, character.MapId, chosen);
        session.Event = run;
        SendRecord(session, run, new EventResult(3, 0, 0, 0, 0, chosen)); // 3 Kind
        run.PendingBattle = chosen;
        run.BattleOutcome = null;
        return true;
    }

    /// <summary>The source name given to runs started by a field encounter.</summary>
    public const string FieldSource = "field encounter";
}
