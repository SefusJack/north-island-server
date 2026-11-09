using NorthIsland.Protocol.Opcodes;

namespace NorthIsland.Server.Actions;

/// <summary>Registers the routes of the packets the client sends with action 9: character creation.</summary>
/// <param name="login">The <c>LoginHandlers</c> that serve the packets of this action.</param>
public sealed class Action09CreateCharacter(
    LoginHandlers login) : IAction
{
    public byte Code => Action9.Code;

    public void Register(ActionRoutes routes) => routes
        .On(Action9.Create, "create character", login.OnCreateCharacter) // 9/1: create a character with the chosen name and looks
        .On(Action9.CheckName, "check name", login.OnCheckName); // 9/2: check whether a character name can be used
}
