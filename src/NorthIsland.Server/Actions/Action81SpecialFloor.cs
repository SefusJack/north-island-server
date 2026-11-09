namespace NorthIsland.Server.Actions;

/// <summary>Registers the routes of the packets the client sends with action 81: special floor things.</summary>
public sealed class Action81SpecialFloor : IAction
{
    public byte Code => 81;

    public void Register(ActionRoutes routes)
    {
    }
}
