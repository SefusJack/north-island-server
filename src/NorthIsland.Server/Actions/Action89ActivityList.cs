using NorthIsland.Protocol.Opcodes;

namespace NorthIsland.Server.Actions;

/// <summary>Registers the routes of the packets the client sends with action 89: the 2014 activity list.</summary>
/// <param name="activity">The <c>ActivityHandlers</c> that serve the packets of this action.</param>
public sealed class Action89ActivityList(
    ActivityHandlers activity) : IAction
{
    public byte Code => Action89.Code;

    public void Register(ActionRoutes routes) => routes
        .On(Action89.ActivityList, "activity list 89/0", activity.OnList) // 89/0: ask for the activity list
        .On(Action89.Teleport, "activity teleport 89/1", activity.OnTeleport); // 89/1: teleport to an activity
}
