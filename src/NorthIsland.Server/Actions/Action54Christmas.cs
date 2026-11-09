namespace NorthIsland.Server.Actions;

/// <summary>Registers the routes of the packets the client sends with action 54: a bare 54.</summary>
public sealed class Action54Christmas : IAction
{
    public byte Code => 54;

    public void Register(ActionRoutes routes)
    {
    }
}
