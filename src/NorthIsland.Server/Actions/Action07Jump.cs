namespace NorthIsland.Server.Actions;

/// <summary>Registers the routes of the packets the client sends with action 7: the debug teleport.</summary>
public sealed class Action07Jump : IAction
{
    public byte Code => 7;

    public void Register(ActionRoutes routes)
    {
    }
}
