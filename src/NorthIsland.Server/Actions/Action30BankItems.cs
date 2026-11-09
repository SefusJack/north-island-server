using NorthIsland.Protocol.Messages.Items;
using NorthIsland.Protocol.Opcodes;

namespace NorthIsland.Server.Actions;

/// <summary>Registers the routes of the packets the client sends with action 30: bank items.</summary>
/// <param name="bank">The <c>BankHandlers</c> that serve the packets of this action.</param>
public sealed class Action30BankItems(
    BankHandlers bank) : IAction
{
    public byte Code => Action30.Code;

    public void Register(ActionRoutes routes) => routes
        .On(Action30.Withdraw, "bank withdraw", bank.OnWithdraw) // 30/1: take stacks out of the bank into the bag
        .On(Action30.Store, "bank store", bank.OnStore) // 30/2: put bag stacks into the bank
        .On(Action30.Move, "bank move", bank.OnMove) // 30/3: move a stack inside the bank
        .On(Action30.DropIn, "bank drop in", bank.OnDropIn) // 30/4: drop a bag item into a bank slot
        .On(Action30.DragOut, "bank drag out", bank.OnDragOut) // 30/5: drag a bank item out into the bag
        .Clears(Action30.Withdraw, () => [BankMessages.WithdrawDone()], whenSilent: true) // 30/1: sends the withdraw done packet if no reply
        .Clears(Action30.Store, () => [BankMessages.StoreDone()], whenSilent: true); // 30/2: sends the store done packet if no reply
}
