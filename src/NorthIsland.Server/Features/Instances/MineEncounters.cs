using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace NorthIsland.Server.Features.Instances;

/// <summary>Starts random fights while a player walks through a mine area inside an instance.</summary>
public sealed class MineEncounters
{
    /// <summary>The map data, which holds the mine areas of a scene.</summary>
    private readonly MapData _maps;

    /// <summary>The world state, used to read the teams.</summary>
    private readonly WorldState _world;

    /// <summary>The registry of live instances; fights only start on instance maps.</summary>
    private readonly InstanceRegistry _instances;

    /// <summary>The event engine that runs the fight of a mine.</summary>
    private readonly EventEngine _events;

    /// <summary>The instance options with the chance and the cooldown.</summary>
    private readonly IOptions<InstanceOptions> _options;

    /// <summary>The world options, for the grace period after arriving on a map.</summary>
    private readonly IOptions<WorldOptions> _worldOptions;

    /// <summary>The logger.</summary>
    private readonly ILogger<MineEncounters> _logger;

    /// <summary>Creates the service and starts the cooldown whenever a mine fight's event finishes.</summary>
    /// <param name="maps">The map data.</param>
    /// <param name="world">The world state.</param>
    /// <param name="instances">The registry of live instances.</param>
    /// <param name="events">The event engine.</param>
    /// <param name="options">The instance options.</param>
    /// <param name="worldOptions">The world options.</param>
    /// <param name="logger">The logger.</param>
    public MineEncounters(
        MapData maps,
        WorldState world,
        InstanceRegistry instances,
        EventEngine events,
        IOptions<InstanceOptions> options,
        IOptions<WorldOptions> worldOptions,
        ILogger<MineEncounters> logger)
    {
        (_maps, _world, _instances, _events, _options, _worldOptions, _logger) = (maps, world, instances, events, options, worldOptions, logger);
        events.EventFinished += (session, run) =>
        {
            if (run.Source.StartsWith(SourcePrefix, StringComparison.Ordinal))
                session.Encounters.MineQuietUntil = GameClock.UtcNow.AddSeconds(options.Value.MineEncounterCooldownSeconds);
        };
    }

    /// <summary>The prefix of the source name of a mine fight's event; it is how the end of such an event is recognised.</summary>
    public const string SourcePrefix = "mine ";

    /// <summary>Walking hook: rolls the chance of a mine fight for a step the player has just made.</summary>
    /// <param name="session">The player's session.</param>
    /// <param name="fromX">The X coordinate the step started at.</param>
    /// <param name="fromY">The Y coordinate the step started at.</param>
    public void OnWalk(GameSession session, int fromX, int fromY) => TryStart(session, fromX, fromY, GameRandom.Shared.Next(100));

    /// <summary>
    /// Starts a mine fight when the player is free, not in cooldown, not following a team leader and on an instance map, the step touches a mine area
    /// of the scene and the roll is below the chance.
    /// </summary>
    /// <param name="session">The player's session.</param>
    /// <param name="fromX">The X coordinate the step started at.</param>
    /// <param name="fromY">The Y coordinate the step started at.</param>
    /// <param name="roll">A random number from 0 to 99, compared with the chance in percent.</param>
    /// <returns>True when a fight was started.</returns>
    public bool TryStart(GameSession session, int fromX, int fromY, int roll)
    {
        var o = _options.Value;
        if (o.MineEncounterPercent <= 0 || session.ActiveCharacter is not { } character || session.Account is null)
            return false;
        var now = GameClock.UtcNow;
        if (!BattleHandlers.IsFree(session) || now < session.Encounters.MineQuietUntil || session.Encounters.Immune(now)
            || session.ArrivedUtc.AddSeconds(Math.Max(_worldOptions.Value.ArrivalGraceSeconds, o.MineEncounterCooldownSeconds)) > now)
            return false;
        var id = session.Account.LoginId;
        if (_world.Teams.LeaderOf(id) is var leader and not 0 && leader != id)
            return false;
        if (_instances.ByMap(character.MapId) is null || _maps.Scene(character.MapId) is not { Mines.Count: > 0 } scene)
            return false;
        var mine = scene.Mines.FirstOrDefault(m => m.Area.Contains(character.X, character.Y) || m.Area.Contains(fromX, fromY));
        if (mine is null || roll >= o.MineEncounterPercent)
            return false;
        if (!_events.StartMine(session, mine))
            return false;
        session.Encounters.MineQuietUntil = now.AddSeconds(o.MineEncounterCooldownSeconds);
        _logger.LogInformation(
            "[{Session}] mine {Mine}: a random fight (next one in {Seconds} s at the earliest)",
            session.Id,
            mine.Index,
            o.MineEncounterCooldownSeconds);
        return true;
    }
}
