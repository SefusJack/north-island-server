namespace NorthIsland.Server.Actions;

/// <summary>Registers the routes of the packets the client sends with action 104: the online draw and the lottery pages.</summary>
/// <param name="onlineDraw">The <c>OnlineDrawHandlers</c> that serve the packets of this action.</param>
public sealed class Action104Lottery(
    OnlineDrawHandlers onlineDraw) : IAction
{
    public byte Code => 104;

    public void Register(ActionRoutes routes) => routes
        .On(1, "online draw", onlineDraw.OnDraw); // 104: draw a prize in the online draw
}
