using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NorthIsland.Protocol.Messages.World;

namespace NorthIsland.Server.Features.Travel;

/// <summary>Serves the trips of the village robot, the submarine and the landing craft, ridden as vehicles.</summary>
/// <param name="routes">The destinations of each vehicle kind.</param>
/// <param name="maps">The map data.</param>
/// <param name="world">The online sessions and maps.</param>
/// <param name="options">The vehicle options; they hold the fuel a trip costs.</param>
/// <param name="logger">The logger.</param>
public sealed class TransmitHandlers(
    TransmitRoutes routes,
    MapData maps,
    WorldState world,
    IOptions<VehicleFeatureOptions> options,
    ILogger<TransmitHandlers> logger)
{
    /// <summary>
    /// Takes the player to the place they picked (<c>69</c>), using fuel when the options ask for it. Refused when they do not ride such a vehicle,
    /// the place does not exist, or in a battle, at a stall, in an event or game.
    /// </summary>
    /// <param name="session">The player's session.</param>
    /// <param name="body">The packet body: u8 place number, 1-based.</param>
    internal ValueTask OnTrip(GameSession session, ReadOnlyMemory<byte> body)
    {
        if (session.State != SessionState.InWorld || session.ActiveCharacter is not { } c || body.Length < 1)
            return ValueTask.CompletedTask;
        var o = options.Value.Vehicles;
        var index = body.Span[0];
        var vehicle = session.Vehicle is { Riding: true } riding ? c.Bag.Find(b => b.Slot == riding.BagSlot) : null;
        var kind = vehicle is null ? 0 : TransmitRoutes.KindOf(vehicle.ItemId);
        var target = routes.Route(kind, index);
        string? refusal = kind == 0 ? "not riding a robot, submarine or landing craft"
            : target is null ? $"no place {index} for kind {kind}"
                : kind == 2 && (maps.SceneType(c.MapId) == 14) != (index == 1) ? "the submarine's pick does not fit the scene" // 2 submarine
                    : session.Battle is not null ? "in a battle"
                        : session.Stalls.Any ? "at a stall"
                            : session.Event is not null || session.Minigame is not null ? "in an event or a game"
                                : null;
        if (refusal is not null)
        {
            logger.LogInformation("[{Session}] {Name}: trip {Index} refused: {Why}", session.Id, c.Name, index, refusal);
            return ValueTask.CompletedTask;
        }
        if (o.TransmitFuel > 0)
        {
            if (vehicle!.Extra < o.TransmitFuel)
            {
                logger.LogInformation(
                    "[{Session}] {Name}: trip {Index} refused: fuel {Fuel} < {Cost}",
                    session.Id,
                    c.Name,
                    index,
                    vehicle.Extra,
                    o.TransmitFuel);
                session.Send(VehicleMessages.NoFuel());
                return ValueTask.CompletedTask;
            }
            vehicle.Extra = (ushort)(vehicle.Extra - o.TransmitFuel);
            VehicleCareHandlers.SendState(session, vehicle);
        }
        logger.LogInformation(
            "[{Session}] {Name} travels by vehicle {Item} to place {Index}: map {Map} ({X}, {Y})",
            session.Id,
            c.Name,
            vehicle!.ItemId,
            index,
            target!.MapId,
            target.X,
            target.Y);
        world.ChangeMap(session, target);
        return ValueTask.CompletedTask;
    }
}
