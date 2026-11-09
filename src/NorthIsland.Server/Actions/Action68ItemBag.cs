using NorthIsland.Protocol.Opcodes;

namespace NorthIsland.Server.Actions;

/// <summary>Registers the routes of the packets the client sends with action 68: the Item Bag.</summary>
/// <param name="miniGrid">The <c>MiniGridHandlers</c> that serve the packets of this action.</param>
public sealed class Action68ItemBag(
    MiniGridHandlers miniGrid) : IAction
{
    public byte Code => Action68.Code;

    public void Register(ActionRoutes routes) => routes
        .On(Action68.ToBag, "item bag take", (s, b) => miniGrid.Order(s, b, Action68.ToBag)) // 68/1: take a stack from the item bag to the bag
        .On(Action68.Move, "item bag move", (s, b) => miniGrid.Order(s, b, Action68.Move)) // 68/2: move a stack inside the item bag
        .On(Action68.ToGrid, "item bag put", (s, b) => miniGrid.Order(s, b, Action68.ToGrid)); // 68/3: put a bag stack into the item bag
}
