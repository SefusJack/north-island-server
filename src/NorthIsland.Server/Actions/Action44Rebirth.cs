namespace NorthIsland.Server.Actions;

/// <summary>Registers the routes of the packets the client sends with action 44: pet rebirth.</summary>
public sealed class Action44Rebirth : IAction
{
    public byte Code => 44;

    public void Register(ActionRoutes routes)
    {
    }
}
