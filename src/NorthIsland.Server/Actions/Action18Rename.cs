namespace NorthIsland.Server.Actions;

/// <summary>Registers the routes of the packets the client sends with action 18: renaming a pet.</summary>
public sealed class Action18Rename : IAction
{
    public byte Code => 18;

    public void Register(ActionRoutes routes)
    {
    }
}
