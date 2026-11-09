using NorthIsland.Protocol.IO;
using NorthIsland.Protocol.Opcodes;

namespace NorthIsland.Protocol.Messages.Items;

/// <summary>Builders for the warehouse packets, all server to client.</summary>
public static class WarehouseMessages
{
    /// <summary>Sends warehouse stacks.</summary>
    /// <param name="entries">The stacks to send.</param>
    /// <returns>The packet.</returns>
    public static byte[] List(IEnumerable<WareEntry> entries)
    {
        var w = new PacketWriter(Action23.Code, Action23.WareList);
        foreach (var e in entries)
            w.U8(e.Slot).U16(e.ItemId).U8(e.Count).U8(e.Wear).U8(0).U8(0);
        return w.ToArray();
    }

    /// <summary>Empties one warehouse slot.</summary>
    /// <param name="slot">The warehouse slot.</param>
    /// <returns>The packet.</returns>
    public static byte[] SlotClear(byte slot) => new PacketWriter(Action23.Code, Action23.WareSlotClear).U8(slot).ToArray();

    /// <summary>Opens the warehouse window.</summary>
    /// <returns>The packet.</returns>
    public static byte[] Open() => new PacketWriter(Action23.Code, Action23.WareOpen).ToArray();
}
