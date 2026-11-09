namespace NorthIsland.Server.Actions;

/// <summary>Registers the routes of the packets the client sends with action 61: the Lotto form.</summary>
public sealed class Action61Lotto : IAction
{
    public byte Code => 61;

    public void Register(ActionRoutes routes)
    {
    }
}
