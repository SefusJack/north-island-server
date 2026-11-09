using NorthIsland.Protocol.Opcodes;

namespace NorthIsland.Server.Actions;

/// <summary>
/// Registers the routes of the packets the client sends with action 5: player status: online state, revival spot, status icons, bookcase and desk.
/// </summary>
/// <param name="autoPlay">The <c>AutoPlayHandlers</c> that serve the packets of this action.</param>
/// <param name="book">The <c>BookHandlers</c> that serve the packets of this action.</param>
/// <param name="timeIcon">The <c>TimeIconHandlers</c> that serve the packets of this action.</param>
/// <param name="world">The <c>WorldHandlers</c> that serve the packets of this action.</param>
public sealed class Action05Status(
    AutoPlayHandlers autoPlay,
    BookHandlers book,
    TimeIconHandlers timeIcon,
    WorldHandlers world) : IAction
{
    public byte Code => Action5.Code;

    public void Register(ActionRoutes routes) => routes
        .On(Action5.OnlineStatus, "online status", world.OnOnlineStatus) // 5/7: sets the friend list status shown to others
        .On(Action5.AfkStop, "afk auto stop", autoPlay.OnLeave) // 5/8: the remote control stopped itself, so the player is taken out
        .On(Action5.BookcasePut, "bookcase put", (s, b) => book.Put(s, b, desk: false)) // 5/9: moves a blueprint from the bag into the bookcase
        .On(Action5.BookcaseTake, "bookcase take", (s, b) => book.Take(s, b, desk: false)) // 5/10: moves a blueprint from the bookcase back into the bag
        .On(Action5.DeskPut, "desk put", (s, b) => book.Put(s, b, desk: true)) // 5/11: moves a blueprint from the bag into the desk
        .On(Action5.DeskTake, "desk take", (s, b) => book.Take(s, b, desk: true)) // 5/12: moves a blueprint from the desk back into the bag
        .On(Action5.ExpClick, "exp icon click", ItemEffectHandlers.OnExpClick) // 5/14: click on the experience icon, asks for the minutes left
        .On(Action5.LuckyGrassClick, "lucky grass icon", TimeIconHandlers.OnLuckyClick) // 5/15: click on the lucky grass icon, asks for the minutes left
        .On(Action5.SpaBunClick, "spa bun icon", timeIcon.OnSpaClick) // 5/16: click on the spa bun icon, asks for the effect state
        .On(Action5.ReturnSpot, "return to revival spot", world.OnReturnSpot); // 5/17: asks to be sent back to the revival spot
}
