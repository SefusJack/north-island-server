using NorthIsland.Protocol.Opcodes;

namespace NorthIsland.Server.Actions;

/// <summary>Registers the routes of the packets the client sends with action 8: spending stat points.</summary>
/// <param name="stat">The <c>StatHandlers</c> that serve the packets of this action.</param>
[System.Diagnostics.CodeAnalysis.SuppressMessage(
    "Naming",
    "CA1711",
    Justification = "The client's name for action 8 (Action08Attribute), kept so the same number has the same name in both codebases.")]
public sealed class Action08Attribute(
    StatHandlers stat) : IAction
{
    public byte Code => Action8.Code;

    public void Register(ActionRoutes routes) => routes
        .On(Action8.SpendStatPoints, "spend stat points", stat.OnSpend); // 8/1: spend free stat points on attributes
}
