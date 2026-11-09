using NorthIsland.Protocol.Opcodes;

namespace NorthIsland.Server.Actions;

/// <summary>Registers the routes of the packets the client sends with action 4: a bare 4.</summary>
public sealed class Action04PlayerPresent : IAction
{
    public byte Code => Action4.Code;

    public void Register(ActionRoutes routes)
    {
    }
}
