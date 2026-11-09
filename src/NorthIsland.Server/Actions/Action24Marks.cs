using NorthIsland.Protocol.Opcodes;

namespace NorthIsland.Server.Actions;

/// <summary>Registers the routes of the packets the client sends with action 24: quest marks.</summary>
/// <param name="social">The <c>SocialHandlers</c> that serve the packets of this action.</param>
public sealed class Action24Marks(
    SocialHandlers social) : IAction
{
    public byte Code => Action24.Code;

    public void Register(ActionRoutes routes) => routes
        .On(Action24.Abandon, "abandon quest", social.OnAbandonQuest) // 24/2: give up a quest
        .On(Action24.MapChangeMarks, "map change with mark flags", WorldHandlers.Accept); // 24/5: map change with mark flags, accepted and ignored
}
