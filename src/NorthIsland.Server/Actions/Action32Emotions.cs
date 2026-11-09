using NorthIsland.Protocol.Opcodes;

namespace NorthIsland.Server.Actions;

/// <summary>Registers the routes of the packets the client sends with action 32: emotions, actions and the yes-no board.</summary>
/// <param name="social">The <c>SocialHandlers</c> that serve the packets of this action.</param>
public sealed class Action32Emotions(
    SocialHandlers social) : IAction
{
    public byte Code => Action32.Code;

    public void Register(ActionRoutes routes) => routes
        .On(Action32.Emotion, "emotion", social.OnEmotion) // 32/1: show an emotion
        .On(Action32.Action, "action", social.OnAction) // 32/2: show an action frame to the others on the map
        .On(Action32.StopAction, "stop action", social.OnStopAction) // 32/3: end the action
        .On(Action32.YesNo, "yes-no board", social.OnYesNo); // 32/4: show the yes or no board
}
