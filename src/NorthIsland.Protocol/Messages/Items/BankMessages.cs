using NorthIsland.Protocol.IO;
using NorthIsland.Protocol.Opcodes;

namespace NorthIsland.Protocol.Messages.Items;

/// <summary>Builders for the bank packets (actions 29 and 30).</summary>
public static class BankMessages
{
    /// <summary>Builds 29/6, which opens the bank item window.</summary>
    /// <returns>The packet.</returns>
    public static byte[] ShowItems() => new PacketWriter(Action29.Code, Action29.ShowItems).ToArray();

    /// <summary>Builds 29/5, which opens the bank money window.</summary>
    /// <returns>The packet.</returns>
    public static byte[] ShowMoney() => new PacketWriter(Action29.Code, Action29.ShowMoney).ToArray();

    /// <summary>Builds 29/4, which sets the gold held in the bank.</summary>
    /// <param name="bankGold">Gold in the bank.</param>
    /// <returns>The packet.</returns>
    public static byte[] Balance(int bankGold) => new PacketWriter(Action29.Code, Action29.Balance).I32(bankGold).ToArray();

    /// <summary>Builds 29/1, which confirms a deposit.</summary>
    /// <param name="amount">Gold deposited.</param>
    /// <returns>The packet.</returns>
    public static byte[] Deposited(int amount) => new PacketWriter(Action29.Code, Action29.Deposited).I32(amount).ToArray();

    /// <summary>Builds 29/2, which confirms a withdrawal.</summary>
    /// <param name="amount">Gold withdrawn.</param>
    /// <returns>The packet.</returns>
    public static byte[] Withdrawn(int amount) => new PacketWriter(Action29.Code, Action29.Withdrawn).I32(amount).ToArray();

    /// <summary>Builds 29/3, which tells the client the bank is full.</summary>
    /// <returns>The packet.</returns>
    public static byte[] Full() => new PacketWriter(Action29.Code, Action29.Full).ToArray();

    /// <summary>Builds the two packets 29/7 and 29/8 that clear the pending state of the money window.</summary>
    /// <returns>The packets, in the order they are sent.</returns>
    public static IEnumerable<byte[]> ClearMoneyPending() =>
        [
            new PacketWriter(Action29.Code, Action29.ClearPending1).ToArray(),
            new PacketWriter(Action29.Code, Action29.ClearPending2).ToArray(),
        ];

    /// <summary>Builds 30/8, which empties the bank item list.</summary>
    /// <returns>The packet.</returns>
    public static byte[] Clear() => new PacketWriter(Action30.Code, Action30.Clear).ToArray();

    /// <summary>Builds 30/1, the whole bank item list.</summary>
    /// <param name="entries">The stacks in the bank.</param>
    /// <returns>The packet.</returns>
    public static byte[] List(IEnumerable<BankEntry> entries)
    {
        var w = new PacketWriter(Action30.Code, Action30.List);
        foreach (var e in entries)
            e.WriteTo(w);
        return w.ToArray();
    }

    /// <summary>Builds 30/4, which puts one stack into the bank.</summary>
    /// <param name="entry">The stack to add.</param>
    /// <returns>The packet.</returns>
    public static byte[] Put(BankEntry entry)
    {
        var w = new PacketWriter(Action30.Code, Action30.Put);
        entry.WriteTo(w);
        return w.ToArray();
    }

    /// <summary>Builds the 30/10 packet that adds items to a given bank slot.</summary>
    /// <param name="entry">The stack to add; its slot says where.</param>
    /// <returns>The packet.</returns>
    public static byte[] PutAtSlot(BankEntry entry) =>
        new PacketWriter(Action30.Code, Action30.PutAtSlot).U16(entry.ItemId).U8(entry.Count).U8(entry.Wear).U8(0).U8(0).U16(entry.Insert)
            .F64(0).U8(0).U8(0).U16(0).U16(0).I32(0).U16(0).U8(entry.Slot).ToArray();

    /// <summary>Builds 30/5, which removes items from a bank slot.</summary>
    /// <param name="slot">Bank slot.</param>
    /// <param name="count">Number of items removed.</param>
    /// <returns>The packet.</returns>
    public static byte[] SlotDecrease(byte slot, byte count) =>
        new PacketWriter(Action30.Code, Action30.SlotDecrease).U8(slot).U8(count).ToArray();

    /// <summary>Builds 30/6, which confirms a withdrawal of an item.</summary>
    /// <returns>The packet.</returns>
    public static byte[] WithdrawDone() => new PacketWriter(Action30.Code, Action30.WithdrawDone).ToArray();

    /// <summary>Builds 30/7, which confirms storing an item.</summary>
    /// <returns>The packet.</returns>
    public static byte[] StoreDone() => new PacketWriter(Action30.Code, Action30.StoreDone).ToArray();
}
