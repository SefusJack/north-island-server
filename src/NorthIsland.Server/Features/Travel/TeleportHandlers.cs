using Microsoft.Extensions.Logging;
using NorthIsland.Protocol.Messages.Items;

namespace NorthIsland.Server.Features.Travel;

/// <summary>Serves the Teleport Scroll and the Dragonflies, which teleport the player to one of the magic map places.</summary>
/// <param name="items">The item records.</param>
/// <param name="world">The online sessions and maps.</param>
/// <param name="logger">The logger.</param>
public sealed class TeleportHandlers(
    ItemData items,
    WorldState world,
    ILogger<TeleportHandlers> logger)
{
    /// <summary>
    /// Teleports the player to a place (<c>23/89</c>), using up a charge of the item. Refused (the bag slot is released) in a battle, at a stall, in
    /// an event or game, in a team, in a house, on the same map or on a map without magic travel.
    /// </summary>
    /// <param name="session">The player's session.</param>
    /// <param name="body">The packet body: u8 place, u8 bag slot, u8 mode (1 scroll, 2 dragonfly).</param>
    internal ValueTask OnUse(GameSession session, ReadOnlyMemory<byte> body)
    {
        if (session.State != SessionState.InWorld || session.ActiveCharacter is not { } c || body.Length < 3)
            return ValueTask.CompletedTask;
        var (place, slot, mode) = (body.Span[0], body.Span[1], body.Span[2]);
        var stack = c.Bag.Find(b => b.Slot == slot);
        var target = MagicMap.Get(place);
        var id = session.Account!.LoginId;
        var valid = stack is not null && target is not null
            && (mode == 1 && stack.ItemId == MagicMap.TeleportScroll || mode == 2 && MagicMap.Dragonflies.Contains(stack.ItemId));
        string? refusal = !valid ? "no such item or place"
            : session.Battle is not null ? "in a battle"
                : session.Stalls.Any ? "at a stall"
                    : session.Event is not null || session.Minigame is not null ? "in an event or a game"
                        : world.Teams.LeaderOf(id) != 0 ? "in a team"
                            : TentHandlers.IsHouseMap(c.MapId) ? "in a house"
                                : MagicMap.NoTeleportMaps.Contains(c.MapId) ? "on a map that cannot be left this way"
                                    : target!.Map == c.MapId ? "on the same map"
                                        : null;
        if (refusal is not null)
        {
            logger.LogInformation(
                "[{Session}] {Name}: teleport (place {Place}, slot {Slot}, mode {Mode}) refused: {Why}",
                session.Id,
                c.Name,
                place,
                slot,
                mode,
                refusal);
            session.Send(ItemMessages.ReleaseSlot(slot));
            return ValueTask.CompletedTask;
        }

        UseLimit.Spend(session, c, stack!, items[stack!.ItemId]!);
        logger.LogInformation("[{Session}] {Name} teleports to {Place} (map {Map})", session.Id, c.Name, target!.Name, target.Map);
        world.ChangeMap(session, new DoorWarp(target.Map, target.X, target.Y, 0));
        return ValueTask.CompletedTask;
    }
}
