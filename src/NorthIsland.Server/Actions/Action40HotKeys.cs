using NorthIsland.Protocol.Opcodes;

namespace NorthIsland.Server.Actions;

/// <summary>Registers the routes of the packets the client sends with action 40: the hotkey bar.</summary>
public sealed class Action40HotKeys : IAction
{
    public byte Code => Action40.Code;

    public void Register(ActionRoutes routes) => routes
        .On(Action40.Slot, "hotkey", SocialHandlers.OnHotkey); // 40/1: store a hotkey slot
}
