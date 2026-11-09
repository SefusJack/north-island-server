using NorthIsland.Protocol.Opcodes;

namespace NorthIsland.Server.Actions;

/// <summary>Registers the routes of the packets the client sends with action 86: the Lost Property Office.</summary>
/// <param name="lostProperty">The <c>LostPropertyHandlers</c> that serve the packets of this action.</param>
public sealed class Action86Property(
    LostPropertyHandlers lostProperty) : IAction
{
    public byte Code => Action86.Code;

    public void Register(ActionRoutes routes) => routes
        .On(Action86.Reclaim, "reclaim lost property", lostProperty.OnReclaim); // 86/2: claim lost property, always refused
}
