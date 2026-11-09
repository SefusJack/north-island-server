namespace NorthIsland.Server.Actions;

/// <summary>Registers the routes of the packets the client sends with action 78: the MK_Computer game results.</summary>
public sealed class Action78Computer : IAction
{
    public byte Code => 78;

    public void Register(ActionRoutes routes)
    {
    }
}
