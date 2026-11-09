using NorthIsland.Protocol.Opcodes;

namespace NorthIsland.Server.Actions;

/// <summary>Registers the routes of the packets the client sends with action 74: GPS.</summary>
/// <param name="gps">The <c>GpsHandlers</c> that serve the packets of this action.</param>
public sealed class Action74Gps(
    GpsHandlers gps) : IAction
{
    public byte Code => Action74.Code;

    public void Register(ActionRoutes routes) => routes
        .On(Action74.Mark, "gps mark", gps.OnMark) // 74/1: set a mark on the GPS
        .On(Action74.Teleport, "gps teleport", gps.OnTeleport) // 74/2: teleport to a GPS mark
        .On(Action74.Upgrade, "gps upgrade", gps.OnUpgrade); // 74/4: upgrade a GPS to the next tier with a crystal
}
