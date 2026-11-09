using NorthIsland.Protocol.Opcodes;

namespace NorthIsland.Server.Actions;

/// <summary>Registers the routes of the packets the client sends with action 79: rebirth and career.</summary>
/// <param name="career">The <c>CareerHandlers</c> that serve the packets of this action.</param>
public sealed class Action79Career(
    CareerHandlers career) : IAction
{
    public byte Code => Action79.Code;

    public void Register(ActionRoutes routes) => routes
        .On(Action79.Choose, "choose career (rebirth)", career.OnChoose) // 79/1: choose a job at rebirth
        .On(Action79.Change, "change career", career.OnChange); // 79/2: change job with a transfer mirror
}
