using NorthIsland.Protocol.IO;
using NorthIsland.Protocol.Opcodes;

namespace NorthIsland.Protocol.Messages.Events;

/// <summary>Builders for the mark packets (action 24): marks a player holds and the Mark.dat flag bits.</summary>
public static class MarkMessages
{
    /// <summary>Builds 24/1, which gives the player marks.</summary>
    /// <param name="markId">Id of the mark.</param>
    /// <param name="count">How many are added.</param>
    /// <returns>The packet.</returns>
    public static byte[] Add(ushort markId, byte count) => new PacketWriter(Action24.Code, Action24.Add).U16(markId).U8(count).ToArray();

    /// <summary>Builds 24/4, which removes a mark.</summary>
    /// <param name="markId">Id of the mark.</param>
    /// <returns>The packet.</returns>
    public static byte[] Delete(ushort markId) => new PacketWriter(Action24.Code, Action24.Delete).U16(markId).ToArray();

    /// <summary>Builds 24/5, which sets or clears a flag bit.</summary>
    /// <param name="flagBit">The flag bit number.</param>
    /// <param name="on">True to set the bit, false to clear it.</param>
    /// <returns>The packet.</returns>
    public static byte[] SetFlag(ushort flagBit, bool on) =>
        new PacketWriter(Action24.Code, Action24.SetFlag).U16(flagBit).U8(on ? (byte)1 : (byte)0).ToArray();

    /// <summary>Builds the packet that sends all flag bits that are set.</summary>
    /// <param name="flagBits">Numbers of the set bits; values outside 1 to 1000 are ignored.</param>
    /// <returns>The packet.</returns>
    public static byte[] Flags(IEnumerable<ushort> flagBits)
    {
        var bytes = new SortedDictionary<int, byte>();
        foreach (var bit in flagBits.Where(b => b is > 0 and <= 1000))
        {
            var index = (bit + 7) / 8;
            bytes[index] = (byte)(bytes.GetValueOrDefault(index) | (1 << ((bit - 1) % 8)));
        }
        var w = new PacketWriter(Action24.Code, Action24.Flags);
        foreach (var (index, value) in bytes)
            w.U8((byte)index).U8(value);
        return w.ToArray();
    }

    /// <summary>Builds the packet with the player's marks, numbered from slot 1 and capped at 200.</summary>
    /// <param name="marks">Mark id and count of each mark.</param>
    /// <returns>The packet.</returns>
    public static byte[] List(IEnumerable<(ushort MarkId, byte Count)> marks)
    {
        var w = new PacketWriter(Action24.Code, Action24.List);
        byte slot = 0;
        foreach (var (id, count) in marks)
        {
            if (++slot > 200)
                break;
            w.U8(slot).U16(id).U8(count);
        }
        return w.ToArray();
    }
}
