using NorthIsland.Protocol.Opcodes;

namespace NorthIsland.Server.Actions;

/// <summary>Registers the routes of the packets the client sends with action 6: walking.</summary>
/// <param name="world">The <c>WorldHandlers</c> that serve the packets of this action.</param>
public sealed class Action06Walk(
    WorldHandlers world) : IAction
{
    public byte Code => Action6.Code;

    public void Register(ActionRoutes routes) => routes
        .On(Action6.WalkTo, "walk to", world.OnWalk) // 6/1: walk to a new position
        .On(Action6.Stop, "stop walk", world.OnWalk); // 6/2: stop walking at the current position
}
