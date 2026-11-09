namespace NorthIsland.Server.Actions;

/// <summary>Registers the routes of the packets the client sends with action 199: GM tools.</summary>
public sealed class Action199Gm : IAction
{
    public byte Code => 199;

    public void Register(ActionRoutes routes)
    {
    }
}
