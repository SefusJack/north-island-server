using NorthIsland.Protocol.Opcodes;

namespace NorthIsland.Server.Actions;

/// <summary>Registers the routes of the packets the client sends with action 26: the gold form.</summary>
public sealed class Action26Gold : IAction
{
    public byte Code => Action26.Code;

    public void Register(ActionRoutes routes)
    {
    }
}
