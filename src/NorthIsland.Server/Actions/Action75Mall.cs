using NorthIsland.Protocol.Opcodes;

namespace NorthIsland.Server.Actions;

/// <summary>Registers the routes of the packets the client sends with action 75: the item mall.</summary>
/// <param name="mall">The <c>MallHandlers</c> that serve the packets of this action.</param>
/// <param name="carnival">The <c>CarnivalHandlers</c> that serve the packets of this action.</param>
public sealed class Action75Mall(
    MallHandlers mall,
    CarnivalHandlers carnival) : IAction
{
    public byte Code => Action75.Code;

    public void Register(ActionRoutes routes) => routes
        .On(Action75.Buy, "mall buy", mall.OnMallBuy) // 75/1: buy the item mall cart lines
        .On(Action75.Forge, "mall forge", mall.OnForge) // 75/3: forge in the item mall, answered with a failure
        .On(Action75.PlayGame, "mall game", carnival.OnMallGame) // 75/4: play a game from the item mall game tab
        .On(Action75.BonusBuy, "mall bonus buy", mall.OnBonusBuy); // 75/5: buy a line of the bonus list with bonus points
}
