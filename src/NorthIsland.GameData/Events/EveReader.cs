using System.Buffers.Binary;

namespace NorthIsland.GameData.Events;

/// <summary>A forward-only little-endian reader over a scene blob that throws <see cref="EveFormatException"/> on overrun.</summary>
/// <param name="data">The bytes to read.</param>
public ref struct EveReader(
    ReadOnlySpan<byte> data)
{
    /// <summary>The bytes being read.</summary>
    private readonly ReadOnlySpan<byte> _data = data;

    /// <summary>The offset of the next byte to read.</summary>
    public int Position { get; private set; }

    /// <summary>The number of bytes left.</summary>
    public readonly int Remaining => _data.Length - Position;

    /// <summary>Reads an unsigned byte.</summary>
    /// <returns>The value.</returns>
    public byte U8() => Take(1)[0];

    /// <summary>Reads an unsigned 16-bit integer.</summary>
    /// <returns>The value.</returns>
    public ushort U16() => BinaryPrimitives.ReadUInt16LittleEndian(Take(2));

    /// <summary>Reads a signed 32-bit integer.</summary>
    /// <returns>The value.</returns>
    public int I32() => BinaryPrimitives.ReadInt32LittleEndian(Take(4));

    /// <summary>Reads a 64-bit floating point number.</summary>
    /// <returns>The value.</returns>
    public double F64() => BinaryPrimitives.ReadDoubleLittleEndian(Take(8));

    /// <summary>Reads a run of raw bytes.</summary>
    /// <param name="count">The number of bytes.</param>
    /// <returns>A view of the bytes.</returns>
    public ReadOnlySpan<byte> Bytes(int count) => Take(count);

    /// <summary>Reads a fixed-size length-prefixed string field and decodes it as code page 950.</summary>
    /// <param name="size">The field size in bytes including the length byte.</param>
    /// <returns>The decoded text.</returns>
    public string ShortString(int size)
    {
        var raw = Take(size);
        var length = Math.Min(raw[0], size - 1);
        return EveText.Current.GetString(raw.Slice(1, length));
    }

    /// <summary>Reads a count byte followed by that many bytes.</summary>
    /// <param name="max">The largest count accepted.</param>
    /// <param name="what">The list name used in the error message.</param>
    /// <returns>The bytes.</returns>
    public byte[] ByteList(int max, string what)
    {
        int count = U8();
        Check(count <= max, $"{what}: {count} entries, the client holds at most {max}");
        return Take(count).ToArray();
    }

    /// <summary>Throws <see cref="EveFormatException"/>, with the current offset, when a condition does not hold.</summary>
    /// <param name="condition">The condition that must hold.</param>
    /// <param name="message">The error text.</param>
    public readonly void Check(bool condition, string message)
    {
        if (!condition)
            throw new EveFormatException($"{message} (offset {Position}).");
    }

    /// <summary>Advances over the next bytes and returns them.</summary>
    /// <param name="count">The number of bytes.</param>
    /// <returns>A view of the bytes.</returns>
    private ReadOnlySpan<byte> Take(int count)
    {
        if (count > Remaining)
            throw new EveFormatException($"Needed {count} byte(s) at offset {Position}, only {Remaining} left.");
        var slice = _data.Slice(Position, count);
        Position += count;
        return slice;
    }
}
