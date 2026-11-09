using NorthIsland.Protocol.Opcodes;

namespace NorthIsland.Server.Actions;

/// <summary>Registers the routes of the packets the client sends with action 76: the maid in the house.</summary>
/// <param name="maid">The <c>MaidHandlers</c> that serve the packets of this action.</param>
public sealed class Action76Maid(
    MaidHandlers maid) : IAction
{
    public byte Code => Action76.Code;

    public void Register(ActionRoutes routes) => routes
        .On(Action76.Bank, "maid bank", (s, b) => maid.OnOrder(s, b, Action76.Bank)) // 76/1: use the maid's bank
        .On(Action76.Pets, "maid pets", (s, b) => maid.OnOrder(s, b, Action76.Pets)) // 76/2: use the maid's pet storage
        .On(Action76.Costume, "maid costume", (s, b) => maid.OnOrder(s, b, Action76.Costume)) // 76/3: change clothes at the maid
        .On(Action76.Hide, "maid menu closed", MaidHandlers.OnClosed); // 76/250: close the maid's menu
}
