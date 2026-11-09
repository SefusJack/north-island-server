namespace NorthIsland.Server.Actions;

/// <summary>Registers the routes of the packets the client sends with action 60: chess.</summary>
public sealed class Action60Chess : IAction
{
    public byte Code => 60;

    public void Register(ActionRoutes routes)
    {
    }
}
