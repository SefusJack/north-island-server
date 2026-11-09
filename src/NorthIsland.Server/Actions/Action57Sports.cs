using NorthIsland.Protocol.Opcodes;

namespace NorthIsland.Server.Actions;

/// <summary>
/// Registers the routes of the packets the client sends with action 57: minigames: results, the quiz, lucky cards, gobang, lucky wheel, Golden
/// Compass.
/// </summary>
/// <param name="events">The <c>EventHandlers</c> that serve the packets of this action.</param>
/// <param name="carnival">The <c>CarnivalHandlers</c> that serve the packets of this action.</param>
/// <param name="compass">The <c>CompassHandlers</c> that serve the packets of this action.</param>
/// <param name="gobang">The <c>GobangHandlers</c> that serve the packets of this action.</param>
/// <param name="quiz">The <c>QuizHandlers</c> that serve the packets of this action.</param>
public sealed class Action57Sports(
    EventHandlers events,
    CarnivalHandlers carnival,
    CompassHandlers compass,
    GobangHandlers gobang,
    QuizHandlers quiz
) : IAction
{
    public byte Code => Action57.Code;

    public void Register(ActionRoutes routes) => routes
        .On(Action57.Result, "minigame result", events.OnMinigameResult) // 57/1: the result of a minigame
        .On(Action57.Quiz, "Q&A", quiz.OnQuizPacket) // 57/8: a packet of the question and answer game
        .On(Action57.Poke, "lucky cards", carnival.OnPokePacket) // 57/9: a packet of the lucky cards game
        .On(Action57.Compass, "Golden Compass", compass.OnPacket) // 57/10: a packet of the Golden Compass game
        .On(Action57.Gobang, "five in a row", gobang.OnPacket) // 57/11: a packet of the five in a row game
        .On(Action57.Wheel, "lucky wheel", carnival.OnWheelPacket); // 57/12: a packet of the lucky wheel
}
