using NorthIsland.Protocol.Opcodes;

namespace NorthIsland.Server.Actions;

/// <summary>Registers the routes of the packets the client sends with action 2: chat on the seven channels.</summary>
/// <param name="world">The <c>WorldHandlers</c> that serve the packets of this action.</param>
public sealed class Action02Chat(
    WorldHandlers world) : IAction
{
    public byte Code => Action2.Code;

    public void Register(ActionRoutes routes)
    {
        foreach (var channel in new[] { Action2.World, Action2.Local, Action2.Whisper, Action2.Channel4, Action2.Team, Action2.Guild, Action2.Channel7 })
            routes.On(channel, $"chat 2/{channel}", (s, body) => world.OnChat(s, channel, body)); // 2/channel: a chat line on one of the seven channels
    }
}
