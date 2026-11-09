using NorthIsland.Protocol.Messages.Events;
using NorthIsland.Protocol.Opcodes;

namespace NorthIsland.Server.Actions;

/// <summary>Registers the routes of the packets the client sends with action 20: NPC events.</summary>
/// <param name="events">The <c>EventHandlers</c> that serve the packets of this action.</param>
public sealed class Action20Event(
    EventHandlers events) : IAction
{
    public byte Code => Action20.Code;

    public void Register(ActionRoutes routes) => routes
        .On("event", events.OnEvent) // 20: an event step: a result request, a dialog answer or a trigger
        .Clears(() => [EventMessages.Finished()], whenSilent: false); // 20: sends the event-finished packet if no reply
}
