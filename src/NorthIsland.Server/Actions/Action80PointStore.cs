using NorthIsland.Protocol.Opcodes;

namespace NorthIsland.Server.Actions;

/// <summary>Registers the routes of the packets the client sends with action 80: the point store.</summary>
/// <param name="mall">The <c>MallHandlers</c> that serve the packets of this action.</param>
public sealed class Action80PointStore(
    MallHandlers mall) : IAction
{
    public byte Code => Action80.Code;

    public void Register(ActionRoutes routes) => routes
        .On(Action80.Buy, "point store buy", mall.OnPointStoreBuy) // 80/1: buy rows of the point store
        .On(Action80.BuyLevel, "point store buy level", mall.OnPointStoreBuyLevel) // 80/2: spend game points on a player level
        .On(Action80.BuyPetLevel, "point store buy pet level", mall.OnPointStoreBuyLevel); // 80/3: spend game points on a pet level
}
