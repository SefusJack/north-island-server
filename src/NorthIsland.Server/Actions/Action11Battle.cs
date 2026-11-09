using NorthIsland.Protocol.Opcodes;

namespace NorthIsland.Server.Actions;

/// <summary>Registers the routes of the packets the client sends with action 11: starting, requesting and joining battles.</summary>
/// <param name="battle">The <c>BattleHandlers</c> that serve the packets of this action.</param>
public sealed class Action11Battle(
    BattleHandlers battle) : IAction
{
    public byte Code => Action11.Code;

    public void Register(ActionRoutes routes) => routes
        .On(Action11.StartEventBattle, "start event battle", battle.OnStartEventBattle) // 11/1: start a battle with an event monster
        .On(Action11.BattleRequest, "battle request", battle.OnBattleRequest) // 11/2: ask for a battle with a monster or another player
        .On(Action11.JoinBattle, "join battle", battle.OnJoinBattle); // 11/3: join a fight that is going on
}
