using System.Buffers.Binary;
using NorthIsland.Protocol.Text;

namespace NorthIsland.Protocol.IO;

/// <summary>
/// Forward-only reader over a packet body: little-endian numbers and strings with a one-byte length. Reading past the end throws <see
/// cref="PacketFormatException"/>.
/// </summary>
/// <param name="data">The bytes to read.</param>
public ref struct PacketReader(
    ReadOnlySpan<byte> data)
{
    /// <summary>The packet bytes being read.</summary>
    private readonly ReadOnlySpan<byte> _data = data;

    /// <summary>Offset of the next unread byte.</summary>
    public int Position { get; private set; }

    /// <summary>Number of bytes not read yet.</summary>
    public readonly int Remaining => _data.Length - Position;

    /// <summary>True when every byte has been read.</summary>
    public readonly bool IsEmpty => Remaining == 0;

    /// <summary>Reads one unsigned byte.</summary>
    /// <returns>The value.</returns>
    public byte U8() => Take(1)[0];

    /// <summary>Reads an unsigned 16-bit little-endian number.</summary>
    /// <returns>The value.</returns>
    public ushort U16() => BinaryPrimitives.ReadUInt16LittleEndian(Take(2));

    /// <summary>Reads a signed 32-bit little-endian number.</summary>
    /// <returns>The value.</returns>
    public int I32() => BinaryPrimitives.ReadInt32LittleEndian(Take(4));

    /// <summary>Reads a block of raw bytes.</summary>
    /// <param name="count">Number of bytes to read.</param>
    /// <returns>A view of the next <paramref name="count"/> bytes of the packet.</returns>
    public ReadOnlySpan<byte> Bytes(int count) => Take(count);

    /// <summary>Reads a string that is prefixed by a one-byte length.</summary>
    /// <returns>The decoded text.</returns>
    public string PStr8() => GameEncoding.GetString(Take(U8()));

    /// <summary>Reads all remaining bytes as a string.</summary>
    /// <returns>The decoded text.</returns>
    public string RestString() => GameEncoding.GetString(Take(Remaining));

    /// <summary>Moves past <paramref name="count"/> bytes and returns them; throws <see cref="PacketFormatException"/> when fewer remain.</summary>
    /// <param name="count">Number of bytes to take.</param>
    /// <returns>A view of the taken bytes.</returns>
    private ReadOnlySpan<byte> Take(int count)
    {
        if (count > Remaining)
            throw new PacketFormatException($"Needed {count} byte(s) at offset {Position}, only {Remaining} left.");
        var slice = _data.Slice(Position, count);
        Position += count;
        return slice;
    }
}
