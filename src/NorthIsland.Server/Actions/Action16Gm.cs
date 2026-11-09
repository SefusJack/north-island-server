namespace NorthIsland.Server.Actions;

/// <summary>Registers the routes of the packets the client sends with action 16: GM commands.</summary>
public sealed class Action16Gm : IAction
{
    public byte Code => 16;

    public void Register(ActionRoutes routes)
    {
    }
}
