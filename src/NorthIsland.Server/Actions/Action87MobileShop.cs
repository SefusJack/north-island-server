using NorthIsland.Protocol.Opcodes;

namespace NorthIsland.Server.Actions;

/// <summary>Registers the routes of the packets the client sends with action 87: the mobile shop subscription.</summary>
public sealed class Action87MobileShop : IAction
{
    public byte Code => Action87.Code;

    public void Register(ActionRoutes routes)
    {
    }
}
