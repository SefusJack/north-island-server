using NorthIsland.Protocol.Opcodes;

namespace NorthIsland.Server.Actions;

/// <summary>Registers the routes of the packets the client sends with action 91: the Lucky Bag preview.</summary>
/// <param name="mallBox">The <c>MallBoxHandlers</c> that serve the packets of this action.</param>
public sealed class Action91LuckyBag(
    MallBoxHandlers mallBox) : IAction
{
    public byte Code => Action91.Code;

    public void Register(ActionRoutes routes) => routes
        .On(Action91.Request, "lucky bag preview", mallBox.OnPreview); // 91/1: preview the rewards of a lucky bag
}
