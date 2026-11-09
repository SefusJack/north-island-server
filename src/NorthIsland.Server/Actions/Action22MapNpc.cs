using NorthIsland.Protocol.Opcodes;

namespace NorthIsland.Server.Actions;

/// <summary>Registers the routes of the packets the client sends with action 22: map NPC frames.</summary>
public sealed class Action22MapNpc : IAction
{
    public byte Code => Action22.Code;

    public void Register(ActionRoutes routes)
    {
    }
}
