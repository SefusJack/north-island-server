using NorthIsland.Protocol.Opcodes;

namespace NorthIsland.Server.Actions;

/// <summary>Registers the routes of the packets the client sends with action 71: the game hall machines.</summary>
/// <param name="carnival">The <c>CarnivalHandlers</c> that serve the packets of this action.</param>
public sealed class Action71Machines(
    CarnivalHandlers carnival) : IAction
{
    public byte Code => Action71.Code;

    public void Register(ActionRoutes routes)
    {
        routes
            .On(LuckyCards.Kind, "lucky cards play", carnival.OnPokePlay) // 71/16: play lucky cards
            .On(LuckyWheel.Kind, "lucky wheel spin", carnival.OnWheelSpin); // 71/21: spin the lucky wheel
        foreach (var game in CarnivalHandlers.Machines)
            routes.On(game, "game hall play", (s, body) => carnival.OnPlay(s, game, body)); // 71/?: play a game hall machine of the game kind
    }
}
