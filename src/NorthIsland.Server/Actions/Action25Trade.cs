using NorthIsland.Protocol.Opcodes;

namespace NorthIsland.Server.Actions;

/// <summary>Registers the routes of the packets the client sends with action 25: player trade, pet trade, large-item trade and pet express.</summary>
/// <param name="express">The <c>ExpressHandlers</c> that serve the packets of this action.</param>
/// <param name="petTrade">The <c>PetTradeHandlers</c> that serve the packets of this action.</param>
/// <param name="trade">The <c>TradeHandlers</c> that serve the packets of this action.</param>
public sealed class Action25Trade(
    ExpressHandlers express,
    PetTradeHandlers petTrade,
    TradeHandlers trade) : IAction
{
    public byte Code => Action25.Code;

    public void Register(ActionRoutes routes) => routes
        .On(Action25.Request, "trade request", trade.OnRequest) // 25/1: ask another player for a trade
        .On(Action25.Offer, "trade offer", trade.OnOffer) // 25/2: put items or gold into the trade offer
        .On(Action25.Decide, "trade decide", trade.OnDecide) // 25/3: accept or cancel the trade
        .On(Action25.PetRequest, "pet trade request", petTrade.OnRequest) // 25/10: ask another player for a pet trade
        .On(Action25.PetOffer, "pet trade offer", petTrade.OnOffer) // 25/11: put pets into the pet trade offer
        .On(Action25.PetDecide, "pet trade decide", petTrade.OnDecide) // 25/12: accept or cancel the pet trade
        .On(Action25.Express, "pet express", express.OnStart) // 25/20: start a pet express trip
        .On(Action25.ExpressStop, "stop pet express", ExpressHandlers.OnStop) // 25/21: end the express trip early
        .On(Action25.LargeRequest, "large-item trade request", (s, b) => trade.OnRequest(s, b, large: true)) // 25/40: ask another player for a large item trade
        .On(Action25.LargeOffer, "large-item trade offer", trade.OnOffer) // 25/41: put large items into the trade offer
        .On(Action25.LargeDecide, "large-item trade decide", trade.OnDecide); // 25/42: accept or cancel the large item trade
}
