using NorthIsland.Protocol.Opcodes;

namespace NorthIsland.Server.Actions;

/// <summary>Registers the routes of the packets the client sends with action 66: the reborn wish.</summary>
/// <param name="rebornWish">The <c>RebornWishHandlers</c> that serve the packets of this action.</param>
public sealed class Action66Reborn(
    RebornWishHandlers rebornWish) : IAction
{
    public byte Code => Action66.Code;

    public void Register(ActionRoutes routes) => routes
        .On(Action66.Wish, "reborn wish", rebornWish.OnWish); // 66/11: wish for a pet from the reborn list
}
