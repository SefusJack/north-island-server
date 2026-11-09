namespace NorthIsland.Server.Actions;

/// <summary>Registers the routes of the packets the client sends with action 67: the debug menu prize text.</summary>
public sealed class Action67Prizes : IAction
{
    public byte Code => 67;

    public void Register(ActionRoutes routes)
    {
    }
}
