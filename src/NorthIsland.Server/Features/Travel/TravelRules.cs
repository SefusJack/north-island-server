using Microsoft.Extensions.Options;

namespace NorthIsland.Server.Features.Travel;

/// <summary>
/// The one rule every teleport follows (return spot, GPS, Magic Magnet, Teleport Scroll and Dragonflies, vehicle trips and activity teleports):
/// which maps may not be left or reached by teleport, and which states of the player keep them where they are. The client checks the same
/// (IsNoTeleportMapId, cannotTeleport, gpsRefusal, activityTeleportRefused, returnToSpot), each path a part of it; this is their union.
/// </summary>
/// <param name="maps">The map data, for the scene flags, scene types and instance copies.</param>
/// <param name="options">The travel options, for the barred map list.</param>
public sealed class TravelRules(
    MapData maps,
    IOptions<TravelOptions> options)
{
    /// <summary>The scene flag bit (value 4) of the maps that may not be left or reached by teleport, nor left by the return spot.</summary>
    public const byte NoTeleportFlag = 4; // scene flag bit 4: no teleporting, no return

    /// <summary>The scene type of the sea scenes, reached only by submarine.</summary>
    public const byte SeaScene = 14; // scene type 14: water (sea) scene

    /// <summary>
    /// Tells whether no teleport may leave or reach a map: a map of the options' list, a scene with flag 4, a house, an instance copy, an event
    /// arena (the Challenge Rounds have no flag 4) or a castle map.
    /// </summary>
    /// <param name="mapId">The map id.</param>
    /// <returns>True when teleporting out of or into the map is barred.</returns>
    public bool IsBarred(int mapId) =>
        mapId is >= 0 and <= ushort.MaxValue && Array.IndexOf(options.Value.NoTeleportMaps, (ushort)mapId) >= 0
        || (maps.SceneFlags(mapId) & NoTeleportFlag) != 0
        || TentHandlers.IsHouseMap(mapId)
        || maps.InstanceOfMap(mapId) is not null
        || EventArenas.IsArena(mapId)
        || CastleRegistry.IsCastleMap(mapId)
        || CastleRegistry.IsTrojanRoom(mapId);

    /// <summary>Tells whether a map is a sea scene.</summary>
    /// <param name="mapId">The map id.</param>
    /// <returns>True for a sea scene.</returns>
    public bool IsSea(int mapId) => maps.SceneType(mapId) == SeaScene;

    /// <summary>
    /// Gives why the player's state keeps them from teleporting: a battle, watching one, an event, a game, a stall, a trade or fishing.
    /// </summary>
    /// <param name="session">The player's session.</param>
    /// <returns>The reason, or null when nothing holds the player.</returns>
    public static string? BusyRefusal(GameSession session) =>
        session.Battle is not null ? "in a battle"
            : session.Watching is not null ? "watching a battle"
                : session.Event is not null ? "in an event"
                    : session.Minigame is not null ? "in a game"
                        : session.Stalls.Any ? "at a stall"
                            : session.Trades.Items is not null || session.Trades.Pets is not null ? "trading"
                                : session.Activities.Fishing is not null ? "fishing"
                                    : null;

    /// <summary>Gives why the player may not leave their map by teleport now: a busy state, or a map that cannot be left this way.</summary>
    /// <param name="session">The player's session.</param>
    /// <param name="c">The player's character.</param>
    /// <returns>The reason, or null when they may leave.</returns>
    public string? LeaveRefusal(GameSession session, Character c) =>
        BusyRefusal(session) ?? (IsBarred(c.MapId) ? $"map {c.MapId} cannot be left this way" : null);

    /// <summary>Gives why a teleport may not land on a map: a barred map or the sea.</summary>
    /// <param name="mapId">The map id of the destination.</param>
    /// <returns>The reason, or null when the map may be reached.</returns>
    public string? ArrivalRefusal(int mapId) =>
        IsBarred(mapId) || IsSea(mapId) ? $"map {mapId} cannot be reached this way" : null;

    /// <summary>Gives why the player may not teleport from their map to another: the leave rule, then the arrival rule.</summary>
    /// <param name="session">The player's session.</param>
    /// <param name="c">The player's character.</param>
    /// <param name="toMap">The map id of the destination.</param>
    /// <returns>The reason, or null when the teleport may go ahead.</returns>
    public string? TeleportRefusal(GameSession session, Character c, int toMap) => LeaveRefusal(session, c) ?? ArrivalRefusal(toMap);
}
