using NorthIsland.Protocol.Opcodes;

namespace NorthIsland.Server.Actions;

/// <summary>Registers the routes of the packets the client sends with action 84: the Top 100 window.</summary>
/// <param name="ranking">The <c>RankingHandlers</c> that serve the packets of this action.</param>
public sealed class Action84Top100(
    RankingHandlers ranking) : IAction
{
    public byte Code => Action84.Code;

    public void Register(ActionRoutes routes) => routes
        .On(Action84.Request, "top 100", ranking.OnRequest); // 84/1: ask for a page of a top 100 list
}
