namespace NorthIsland.Server.Actions;

/// <summary>Registers the routes of the packets the client sends with action 41: compound facility updates.</summary>
public sealed class Action41Facility : IAction
{
    public byte Code => 41;

    public void Register(ActionRoutes routes)
    {
    }
}
