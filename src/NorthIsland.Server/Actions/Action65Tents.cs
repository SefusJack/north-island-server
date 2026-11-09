using NorthIsland.Protocol.Opcodes;

namespace NorthIsland.Server.Actions;

/// <summary>Registers the routes of the packets the client sends with action 65: tents and house pillars.</summary>
/// <param name="pillar">The <c>PillarHandlers</c> that serve the packets of this action.</param>
/// <param name="tent">The <c>TentHandlers</c> that serve the packets of this action.</param>
public sealed class Action65Tents(
    PillarHandlers pillar,
    TentHandlers tent) : IAction
{
    public byte Code => Action65.Code;

    public void Register(ActionRoutes routes) => routes
        .On(Action65.Enter, "enter tent", tent.OnEnter) // 65/1: click a tent to go in
        .On(Action65.TakeDown, "take down tent", tent.OnTakeDown) // 65/2: take down their own tent from the bag
        .On(Action65.Open, "open the house", (s, b) => tent.OnLock(s, b, locked: false)) // 65/3: open the house by clicking a door-lock carpet
        .On(Action65.Close, "close the house", (s, b) => tent.OnLock(s, b, locked: true)) // 65/4: close the house by clicking a door-lock carpet
        .On(Action65.Pillars, "house pillars", pillar.OnChanges) // 65/11: owner changes the house pillars
        .On(Action65.ChangeStyle, "change tent style", tent.OnChangeStyle); // 65/12: pick a style from a tent palette
}
