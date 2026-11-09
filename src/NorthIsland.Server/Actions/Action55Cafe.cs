namespace NorthIsland.Server.Actions;

/// <summary>Registers the routes of the packets the client sends with action 55: the internet-cafe subscription.</summary>
public sealed class Action55Cafe : IAction
{
    public byte Code => 55;

    public void Register(ActionRoutes routes)
    {
    }
}
