using NorthIsland.Protocol.Opcodes;

namespace NorthIsland.Server.Actions;

/// <summary>Registers the routes of the packets the client sends with action 70: the Trojan war / castle.</summary>
public sealed class Action70Castle : IAction
{
    public byte Code => Action70.Code;

    public void Register(ActionRoutes routes)
    {
    }
}
