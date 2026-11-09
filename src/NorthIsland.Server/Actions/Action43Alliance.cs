using NorthIsland.Protocol.Opcodes;

namespace NorthIsland.Server.Actions;

/// <summary>Registers the routes of the packets the client sends with action 43: alliances.</summary>
public sealed class Action43Alliance : IAction
{
    public byte Code => Action43.Code;

    public void Register(ActionRoutes routes)
    {
    }
}
