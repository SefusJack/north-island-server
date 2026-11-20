namespace NorthIsland.Server.Features.Storage;

/// <summary>The check that a request to an NPC service window (a store, the bank, the ATM) comes from a player who has that window open.</summary>
public static class NpcService
{
    /// <summary>
    /// Whether a player may use an NPC service now: not in a battle, and with an NPC event running on the player's current map. The event that
    /// opens a service window waits until the window closes, so a request without one, or with one of another map, comes from no open window.
    /// </summary>
    /// <param name="session">The player's session.</param>
    /// <param name="character">The player's active character.</param>
    /// <returns>True when the request may be served.</returns>
    public static bool AtNpc(GameSession session, Character character) =>
        session.Battle is null && session.Event is { } run && run.MapId == character.MapId;
}
