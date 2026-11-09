using NorthIsland.Protocol.Opcodes;

namespace NorthIsland.Server.Actions;

/// <summary>Registers the routes of the packets the client sends with action 3: a bare 3 with one byte of the player record.</summary>
public sealed class Action03PlayerEnters : IAction
{
    public byte Code => Action3.Code;

    public void Register(ActionRoutes routes)
    {
    }
}
