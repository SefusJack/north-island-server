namespace NorthIsland.Server.Actions;

/// <summary>Registers the routes of the packets the client sends with action 69: the robot trip.</summary>
/// <param name="transmit">The <c>TransmitHandlers</c> that serve the packets of this action.</param>
public sealed class Action69Trips(
    TransmitHandlers transmit) : IAction
{
    public byte Code => 69;

    public void Register(ActionRoutes routes) => routes
        .On("robot trip", transmit.OnTrip); // 69: travel to a picked place by vehicle
}
