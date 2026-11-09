namespace NorthIsland.Server.Actions;

/// <summary>Registers the routes of the packets the client sends with action 21: the ID / password re-entry form.</summary>
public sealed class Action21IdAndPw : IAction
{
    public byte Code => 21;

    public void Register(ActionRoutes routes)
    {
    }
}
