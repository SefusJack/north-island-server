using NorthIsland.Protocol.Opcodes;

namespace NorthIsland.Server.Actions;

/// <summary>Registers the routes of the packets the client sends with action 37: the server ip echo.</summary>
/// <param name="world">The <c>WorldHandlers</c> that serve the packets of this action.</param>
public sealed class Action37ServerEcho(
    WorldHandlers world) : IAction
{
    public byte Code => Action37.Code;

    public void Register(ActionRoutes routes) => routes
        .On(Action37.ServerIpEcho, "server ip echo", world.OnServerIpEcho); // 37/1: the server address the client reports seeing
}
