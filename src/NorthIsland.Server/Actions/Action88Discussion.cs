namespace NorthIsland.Server.Actions;

/// <summary>Registers the routes of the packets the client sends with action 88: the forum / mobile item list.</summary>
public sealed class Action88Discussion : IAction
{
    public byte Code => 88;

    public void Register(ActionRoutes routes)
    {
    }
}
