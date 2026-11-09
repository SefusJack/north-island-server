using NorthIsland.Protocol.Messages.Items;
using NorthIsland.Protocol.Opcodes;

namespace NorthIsland.Server.Actions;

/// <summary>Registers the routes of the packets the client sends with action 29: bank gold and the ATM.</summary>
/// <param name="atm">The <c>AtmHandlers</c> that serve the packets of this action.</param>
/// <param name="bank">The <c>BankHandlers</c> that serve the packets of this action.</param>
public sealed class Action29BankGold(
    AtmHandlers atm,
    BankHandlers bank) : IAction
{
    public byte Code => Action29.Code;

    public void Register(ActionRoutes routes) => routes
        .On(Action29.Withdraw, "bank withdraw gold", bank.OnWithdrawGold) // 29/1: take gold out of the bank
        .On(Action29.Deposit, "bank deposit gold", bank.OnDeposit) // 29/2: put gold into the bank
        .On(Action29.AtmDeposit, "ATM deposit", atm.OnDeposit) // 29/3: put gold into the ATM
        .On(Action29.AtmWithdraw, "ATM withdraw", atm.OnWithdraw) // 29/4: take gold out of the ATM
        .On(Action29.AtmTransfer, "ATM transfer", atm.OnTransfer) // 29/5: transfer gold with the ATM
        .Clears(Action29.Withdraw, BankMessages.ClearMoneyPending, whenSilent: true) // 29/1: sends the money pending packet if no reply
        .Clears(Action29.Deposit, BankMessages.ClearMoneyPending, whenSilent: true); // 29/2: sends the money pending packet if no reply
}
