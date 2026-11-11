using NorthIsland.Protocol.Opcodes;

namespace NorthIsland.Server.Actions;

/// <summary>Registers the routes of the packets the client sends with action 35: character delete and password change.</summary>
/// <param name="login">The <c>LoginHandlers</c> that serve the packets of this action.</param>
public sealed class Action35Password(
    LoginHandlers login) : IAction
{
    public byte Code => Action35.Code;

    public void Register(ActionRoutes routes) => routes
        .On(Action35.ChangePassword, "change secret code", login.OnChangePassword) // 35/1: change the character's secret code
        .On(Action35.Delete, "delete character", login.OnDeleteCharacter) // 35/2: delete a character
        .On(Action35.ChangeAccountPassword, "change account password", login.OnChangeAccountPassword); // 35/12: change the account password
}
