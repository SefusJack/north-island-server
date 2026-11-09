using NorthIsland.Protocol.Opcodes;

namespace NorthIsland.Server.Actions;

/// <summary>Registers the routes of the packets the client sends with action 50: battle commands.</summary>
/// <param name="battle">The <c>BattleHandlers</c> that serve the packets of this action.</param>
public sealed class Action50BattleActions(
    BattleHandlers battle) : IAction
{
    public byte Code => Action50.Code;

    public void Register(ActionRoutes routes) => routes
        .On(Action50.Skill, "battle command", (s, b) => battle.OnCommand(s, b, isItem: false)) // 50/1: choose a skill command for a fighter
        .On(Action50.Item, "battle item", (s, b) => battle.OnCommand(s, b, isItem: true)); // 50/2: choose an item command for a fighter
}
