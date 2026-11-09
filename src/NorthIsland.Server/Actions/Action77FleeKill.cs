using NorthIsland.Protocol.Opcodes;

namespace NorthIsland.Server.Actions;

/// <summary>Registers the routes of the packets the client sends with action 77: the Escape Button in a battle.</summary>
/// <param name="battle">The <c>BattleHandlers</c> that serve the packets of this action.</param>
public sealed class Action77FleeKill(
    BattleHandlers battle) : IAction
{
    public byte Code => Action77.Code;

    public void Register(ActionRoutes routes) => routes
        .On("escape button", battle.OnEscapeButton); // 77: press the escape button in a battle
}
