using NorthIsland.Protocol.Opcodes;

namespace NorthIsland.Server.Actions;

/// <summary>Registers the routes of the packets the client sends with action 10: nickname and profile.</summary>
/// <param name="social">The <c>SocialHandlers</c> that serve the packets of this action.</param>
public sealed class Action10Names(
    SocialHandlers social) : IAction
{
    public byte Code => Action10.Code;

    public void Register(ActionRoutes routes) => routes
        .On(Action10.Nickname, "set nickname", social.OnNickname) // 10/1: set the nickname
        .On(Action10.Profile, "set profile", social.OnProfile) // 10/2: set the profile (heart number, blood type, birthday)
        .On(Action10.Submitted, "profile submitted", SocialHandlers.Accept); // 10/3: profile submitted, accepted and ignored
}
