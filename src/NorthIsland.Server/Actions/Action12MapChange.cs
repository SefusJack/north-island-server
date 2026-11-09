using NorthIsland.Protocol.Opcodes;

namespace NorthIsland.Server.Actions;

/// <summary>Registers the routes of the packets the client sends with action 12: map change done.</summary>
/// <param name="world">The <c>WorldHandlers</c> that serve the packets of this action.</param>
public sealed class Action12MapChange(
    WorldHandlers world) : IAction
{
    public byte Code => Action12.Code;

    public void Register(ActionRoutes routes) => routes
        .On(Action12.MapChangeDone, "map change done", world.OnMapChangeDone); // 12/1: the client finished changing map
}
