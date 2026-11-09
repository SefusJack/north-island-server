using NorthIsland.Protocol.Opcodes;

namespace NorthIsland.Server.Actions;

/// <summary>Registers the routes of the packets the client sends with action 1: account and session messages.</summary>
public sealed class Action01System : IAction
{
    public byte Code => Action1.Code;

    public void Register(ActionRoutes routes)
    {
    }
}
