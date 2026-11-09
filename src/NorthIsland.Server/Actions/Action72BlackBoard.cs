using NorthIsland.Protocol.Opcodes;

namespace NorthIsland.Server.Actions;

/// <summary>Registers the routes of the packets the client sends with action 72: the house blackboard.</summary>
/// <param name="blackboard">The <c>BlackboardHandlers</c> that serve the packets of this action.</param>
public sealed class Action72BlackBoard(
    BlackboardHandlers blackboard) : IAction
{
    public byte Code => Action72.Code;

    public void Register(ActionRoutes routes) => routes
        .On(Action72.Open, "blackboard", blackboard.OnOpen) // 72/1: open the blackboard and get its rows
        .On(Action72.Write, "blackboard write", blackboard.OnWrite) // 72/2: write a row on the blackboard
        .On(Action72.Erase, "blackboard erase", blackboard.OnErase); // 72/3: erase a row of the blackboard
}
