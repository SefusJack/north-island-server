namespace NorthIsland.Protocol.Opcodes;

/// <summary>Sub codes of action <c>29</c>: the bank's gold and the ATM requests.</summary>
public static class Action29
{
    /// <summary>The action byte <c>29</c> that starts every packet of this group.</summary>
    public const byte Code = 29;

    /// <summary>Client to server: withdraws gold from the bank.</summary>
    public const byte Withdraw = 1;

    /// <summary>Client to server: deposits gold in the bank.</summary>
    public const byte Deposit = 2;

    /// <summary>Server to client: gold was deposited; carries the amount.</summary>
    public const byte Deposited = 1;

    /// <summary>Server to client: gold was withdrawn; carries the amount.</summary>
    public const byte Withdrawn = 2;

    /// <summary>Server to client: the bank cannot hold more gold; no data.</summary>
    public const byte Full = 3;

    /// <summary>Server to client: the gold in the bank.</summary>
    public const byte Balance = 4;

    /// <summary>Server to client: opens the bank's gold form; no data.</summary>
    public const byte ShowMoney = 5;

    /// <summary>Server to client: opens the bank's item form; no data.</summary>
    public const byte ShowItems = 6;

    /// <summary>Server to client: first of two packets that release the client after a gold request that got no reply.</summary>
    public const byte ClearPending1 = 7;

    /// <summary>Server to client: second of two packets that release the client after a gold request that got no reply.</summary>
    public const byte ClearPending2 = 8;

    /// <summary>Client to server: deposits carried gold into the ATM.</summary>
    public const byte AtmDeposit = 3;

    /// <summary>Client to server: withdraws ATM gold into the carried gold.</summary>
    public const byte AtmWithdraw = 4;

    /// <summary>Client to server: transfers ATM gold to another online player's ATM.</summary>
    public const byte AtmTransfer = 5;
}
