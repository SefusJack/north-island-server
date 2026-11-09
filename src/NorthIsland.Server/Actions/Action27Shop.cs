using NorthIsland.Protocol.Opcodes;

namespace NorthIsland.Server.Actions;

/// <summary>Registers the routes of the packets the client sends with action 27: NPC shops.</summary>
/// <param name="shop">The <c>ShopHandlers</c> that serve the packets of this action.</param>
public sealed class Action27Shop(
    ShopHandlers shop) : IAction
{
    public byte Code => Action27.Code;

    public void Register(ActionRoutes routes) => routes
        .On(Action27.Buy, "shop buy", shop.OnBuy) // 27/1: buy from a shop
        .On(Action27.Sell, "shop sell", shop.OnSell); // 27/2: sell bag items to a shop
}
