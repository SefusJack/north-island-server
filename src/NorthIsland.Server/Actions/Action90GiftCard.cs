using NorthIsland.Protocol.Opcodes;

namespace NorthIsland.Server.Actions;

/// <summary>Registers the routes of the packets the client sends with action 90: gift card lists.</summary>
/// <param name="giftCard">The <c>GiftCardHandlers</c> that serve the packets of this action.</param>
public sealed class Action90GiftCard(
    GiftCardHandlers giftCard) : IAction
{
    public byte Code => Action90.Code;

    public void Register(ActionRoutes routes) => routes
        .On(Action90.State, "gift card lists", giftCard.OnLists); // 90/1: ask for the lists of the named gift cards
}
