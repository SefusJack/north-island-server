namespace NorthIsland.Server.Actions;

/// <summary>Registers the routes of the packets the client sends with action 59: the ShiBaDou game.</summary>
public sealed class Action59ShiBaDou : IAction
{
    public byte Code => 59;

    public void Register(ActionRoutes routes)
    {
    }
}
