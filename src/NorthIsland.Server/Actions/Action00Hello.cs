using NorthIsland.Protocol.Opcodes;

namespace NorthIsland.Server.Actions;

/// <summary>Registers the routes of the packets the client sends with action 0: hello / kick acknowledge.</summary>
/// <param name="login">The <c>LoginHandlers</c> that serve the packets of this action.</param>
public sealed class Action00Hello(
    LoginHandlers login) : IAction
{
    public byte Code => Action0.Code;

    public void Register(ActionRoutes routes) => routes
        .On("hello", login.OnHello); // 0: the client's hello at connection start
}
