using NorthIsland.Protocol.Opcodes;

namespace NorthIsland.Server.Actions;

/// <summary>Registers the routes of the packets the client sends with action 34: asking the IM points.</summary>
public sealed class Action34ImPoints : IAction
{
    public byte Code => Action34.Code;

    public void Register(ActionRoutes routes) => routes
        .On(Action34.AskPoints, "ask IM points", MallHandlers.OnAskPoints); // 34/1: ask for the account's mall points
}
