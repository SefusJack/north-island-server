namespace NorthIsland.Protocol.Opcodes;

/// <summary>Sub codes of action <c>30</c>: the bank's items.</summary>
public static class Action30
{
    /// <summary>The action byte <c>30</c> that starts every packet of this group.</summary>
    public const byte Code = 30;

    /// <summary>Client to server: takes an item out of the bank.</summary>
    public const byte Withdraw = 1;

    /// <summary>Client to server: puts an item into the bank.</summary>
    public const byte Store = 2;

    /// <summary>Server to client: the bank's items, as fixed-size records.</summary>
    public const byte List = 1;

    /// <summary>Server to client: one bank record, added to its slot.</summary>
    public const byte Put = 4;

    /// <summary>Server to client: lowers the count in one bank slot.</summary>
    public const byte SlotDecrease = 5;

    /// <summary>Server to client: releases the client after a withdraw request.</summary>
    public const byte WithdrawDone = 6;

    /// <summary>Server to client: releases the client after a store request.</summary>
    public const byte StoreDone = 7;

    /// <summary>Server to client: empties the client's bank.</summary>
    public const byte Clear = 8;

    /// <summary>Client to server: moves, merges or swaps items inside the bank window by drag and drop.</summary>
    public const byte Move = 3;

    /// <summary>Client to server: drags a bag item into the bank window.</summary>
    public const byte DropIn = 4;

    /// <summary>Client to server: drags a bank item out into the bag.</summary>
    public const byte DragOut = 5;

    /// <summary>Server to client: one bank record, added to the slot named after it.</summary>
    public const byte PutAtSlot = 10;
}
