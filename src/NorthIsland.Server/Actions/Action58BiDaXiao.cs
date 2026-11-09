namespace NorthIsland.Server.Actions;

/// <summary>Registers the routes of the packets the client sends with action 58: the BiDaXiao dice bet.</summary>
public sealed class Action58BiDaXiao : IAction
{
    public byte Code => 58;

    public void Register(ActionRoutes routes)
    {
    }
}
