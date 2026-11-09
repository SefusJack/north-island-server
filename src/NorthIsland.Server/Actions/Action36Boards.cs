namespace NorthIsland.Server.Actions;

/// <summary>Registers the routes of the packets the client sends with action 36: the guestbook.</summary>
public sealed class Action36Boards : IAction
{
    public byte Code => 36;

    public void Register(ActionRoutes routes)
    {
    }
}
