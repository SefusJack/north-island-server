namespace NorthIsland.Protocol.Opcodes;

/// <summary>Sub codes of action <c>26</c>: server to client updates of the gold carried and of the ATM balance.</summary>
public static class Action26
{
    /// <summary>The action byte <c>26</c> that starts every packet of this group.</summary>
    public const byte Code = 26;

    /// <summary>Server to client: the carried gold grows by an amount; the client shows "Obtain N Gold".</summary>
    public const byte AddMoney = 1;

    /// <summary>Server to client: the carried gold changes by a negative amount.</summary>
    public const byte SpendMoney = 2;

    /// <summary>Server to client: the money bag is full; no data.</summary>
    public const byte MoneyBagFull = 3;

    /// <summary>Server to client: sets the carried gold.</summary>
    public const byte SetMoney = 4;

    /// <summary>Server to client: a stall trade failed.</summary>
    public const byte StallTradeFailed = 7;

    /// <summary>Server to client: the ATM balance grows by an amount, with the sender id of a transfer.</summary>
    public const byte AtmAdd = 8;

    /// <summary>Server to client: the ATM balance shrinks by an amount, with the target id of a transfer.</summary>
    public const byte AtmSubtract = 9;

    /// <summary>Server to client: sets the ATM balance.</summary>
    public const byte AtmSet = 10;

    /// <summary>Server to client: the ATM cannot hold more gold; no data.</summary>
    public const byte AtmFull = 11;
}
