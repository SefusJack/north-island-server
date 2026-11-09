using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NorthIsland.GameData.Events;

namespace NorthIsland.Server.Features.Battles;

/// <summary>Starts random fights against field monsters as a player walks.</summary>
public sealed class FieldEncounters
{
    /// <summary>Map data for the scenes.</summary>
    private readonly MapData _maps;

    /// <summary>NPC records.</summary>
    private readonly NpcData _npcs;

    /// <summary>The shared world state (teams, monsters out of play).</summary>
    private readonly WorldState _world;

    /// <summary>The registry of instance maps.</summary>
    private readonly InstanceRegistry _instances;

    /// <summary>The event engine that starts the monsters' own battle events.</summary>
    private readonly EventEngine _events;

    /// <summary>The battle options.</summary>
    private readonly IOptions<BattleOptions> _options;

    /// <summary>The world options.</summary>
    private readonly IOptions<WorldOptions> _worldOptions;

    /// <summary>The logger.</summary>
    private readonly ILogger<FieldEncounters> _logger;

    /// <summary>Creates the encounter service and starts the cooldown whenever a field event finishes.</summary>
    /// <param name="maps">Map data for the scenes.</param>
    /// <param name="npcs">NPC records.</param>
    /// <param name="world">The shared world state.</param>
    /// <param name="instances">The registry of instance maps.</param>
    /// <param name="events">The event engine that starts the battle events.</param>
    /// <param name="options">The battle options.</param>
    /// <param name="worldOptions">The world options.</param>
    /// <param name="logger">The logger.</param>
    public FieldEncounters(
        MapData maps,
        NpcData npcs,
        WorldState world,
        InstanceRegistry instances,
        EventEngine events,
        IOptions<BattleOptions> options,
        IOptions<WorldOptions> worldOptions,
        ILogger<FieldEncounters> logger)
    {
        (_maps, _npcs, _world, _instances, _events, _options, _worldOptions, _logger) = (maps, npcs, world, instances, events, options, worldOptions, logger);
        events.EventFinished += (session, run) =>
        {
            if (run.Source == EventEngine.FieldSource)
                session.Encounters.FieldQuietUntil = GameClock.UtcNow.AddSeconds(options.Value.FieldEncounterCooldownSeconds);
        };
    }

    /// <summary>Rolls for a random fight after the player took a step.</summary>
    /// <param name="session">The player's session.</param>
    public void OnWalk(GameSession session) => TryStart(session, GameRandom.Shared.Next(100), GameRandom.Shared);

    /// <summary>Starts a field fight when the roll succeeds and the player is allowed one.</summary>
    /// <param name="session">The player's session.</param>
    /// <param name="roll">The roll from 0 to 99; the fight can start only when it is below <see cref="BattleOptions.FieldEncounterPercent"/>.</param>
    /// <param name="rng">The random source passed on to the event engine to pick a monster.</param>
    /// <returns>True when a fight was started.</returns>
    public bool TryStart(GameSession session, int roll, Random rng)
    {
        var o = _options.Value;
        if (o.FieldEncounterPercent <= 0 || session.ActiveCharacter is not { } character || session.Account is null)
            return false;
        var now = GameClock.UtcNow;
        if (!BattleHandlers.IsFree(session) || now < session.Encounters.FieldQuietUntil || session.Encounters.Immune(now) || session.Trades.Items is not null
            || session.ArrivedUtc.AddSeconds(Math.Max(_worldOptions.Value.ArrivalGraceSeconds, o.FieldEncounterCooldownSeconds)) > now)
            return false;
        var id = session.Account.LoginId;
        if (_world.Teams.LeaderOf(id) is var leader and not 0 && leader != id)
            return false;
        var map = character.MapId;
        if (_instances.ByMap(map) is not null || EventArenas.IsArena(map) || _maps.SceneType(map) is 2 or 3)
            return false;
        if (TimedEffects.EncountersBlocked(character, TimedEffects.Now))
            return false;
        if (session.Vehicle is { Riding: true } ride && !PlacedVehicle.IsRobot(ride.ItemId))
            return false;
        if (roll >= o.FieldEncounterPercent)
            return false;
        var near = Surrounding(session, character);
        if (near.Count == 0 || !_events.StartField(session, near, rng))
            return false;
        session.Encounters.FieldQuietUntil = now.AddSeconds(o.FieldEncounterCooldownSeconds);
        _logger.LogInformation(
            "[{Session}] field encounter on map {Map} against one of {Count} monster(s) around ({X},{Y}); next one in {Seconds} s at the earliest",
            session.Id, map, near.Count, character.X, character.Y, o.FieldEncounterCooldownSeconds);
        return true;
    }

    /// <summary>Collects the field monsters close enough to the player to start a fight.</summary>
    /// <param name="session">The player's session.</param>
    /// <param name="character">The player's active character.</param>
    /// <returns>The scene's monster NPCs in range, possibly empty.</returns>
    private List<EveNpc> Surrounding(GameSession session, Character character)
    {
        var o = _options.Value;
        if (_maps.Scene(character.MapId) is not { } scene)
            return [];
        var now = GameClock.UtcNow;
        List<EveNpc> near = [];
        for (var i = 0; i < scene.Npcs.Count; i++)
        {
            var npc = scene.Npcs[i];
            var role = (ushort)(i + 1);
            if (_npcs[(ushort)npc.NpcId] is not { IsMonster: true }
                || !scene.EventsOf(npc).Any(e => e.IsFieldMonster)
                || _world.MonsterIsOut(character.MapId, role)
                || session.HiddenNpcs.TryGetValue((character.MapId, role), out var hidden) && hidden > now
                || session.NpcStates.TryGetValue(role, out var state) && state is 2 or 4)
                continue;
            var (halfX, halfY) = NpcWander.Roams(npc) ? (npc.Unknown688, npc.Unknown692) : (0, 0);
            var dx = Math.Max(0, Math.Abs(character.X - npc.X) - halfX);
            var dy = Math.Max(0, Math.Abs(character.Y - npc.Y) - halfY);
            if ((long)dx * dx + (long)dy * dy <= (long)o.FieldEncounterRadius * o.FieldEncounterRadius)
                near.Add(npc);
        }
        return near;
    }
}
