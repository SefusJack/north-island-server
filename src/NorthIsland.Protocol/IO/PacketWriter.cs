using System.Buffers;
using System.Buffers.Binary;
using NorthIsland.Protocol.Text;

namespace NorthIsland.Protocol.IO;

/// <summary>Builds a packet body by appending little-endian values; every append method returns the writer so calls can be chained.</summary>
public sealed class PacketWriter
{
    /// <summary>The bytes written so far.</summary>
    private readonly ArrayBufferWriter<byte> _buffer = new(64);

    /// <summary>Creates an empty writer.</summary>
    public PacketWriter() { }

    /// <summary>Creates a writer that starts with the action byte and, when given, the sub code.</summary>
    /// <param name="action">The first packet byte.</param>
    /// <param name="sub">The second packet byte, left out when null.</param>
    public PacketWriter(
        byte action,
        byte? sub = null)
    {
        U8(action);
        if (sub is { } s)
            U8(s);
    }

    /// <summary>Number of bytes written so far.</summary>
    public int Length => _buffer.WrittenCount;

    /// <summary>Appends one unsigned byte.</summary>
    /// <param name="value">The byte to write.</param>
    /// <returns>This writer.</returns>
    public PacketWriter U8(byte value)
    {
        _buffer.GetSpan(1)[0] = value;
        _buffer.Advance(1);
        return this;
    }

    /// <summary>Appends a boolean as one byte, 1 for true and 0 for false.</summary>
    /// <param name="value">The flag to write.</param>
    /// <returns>This writer.</returns>
    public PacketWriter Bool(bool value) => U8(value ? (byte)1 : (byte)0);

    /// <summary>Appends an unsigned 16-bit little-endian number.</summary>
    /// <param name="value">The number to write.</param>
    /// <returns>This writer.</returns>
    public PacketWriter U16(ushort value)
    {
        BinaryPrimitives.WriteUInt16LittleEndian(_buffer.GetSpan(2), value);
        _buffer.Advance(2);
        return this;
    }

    /// <summary>Appends a signed 32-bit little-endian number.</summary>
    /// <param name="value">The number to write.</param>
    /// <returns>This writer.</returns>
    public PacketWriter I32(int value)
    {
        BinaryPrimitives.WriteInt32LittleEndian(_buffer.GetSpan(4), value);
        _buffer.Advance(4);
        return this;
    }

    /// <summary>Appends an unsigned 32-bit little-endian number.</summary>
    /// <param name="value">The number to write.</param>
    /// <returns>This writer.</returns>
    public PacketWriter U32(uint value)
    {
        BinaryPrimitives.WriteUInt32LittleEndian(_buffer.GetSpan(4), value);
        _buffer.Advance(4);
        return this;
    }

    /// <summary>Appends an 8-byte little-endian floating point number.</summary>
    /// <param name="value">The number to write.</param>
    /// <returns>This writer.</returns>
    public PacketWriter F64(double value)
    {
        BinaryPrimitives.WriteDoubleLittleEndian(_buffer.GetSpan(8), value);
        _buffer.Advance(8);
        return this;
    }

    /// <summary>Appends raw bytes.</summary>
    /// <param name="value">The bytes to write.</param>
    /// <returns>This writer.</returns>
    public PacketWriter Bytes(ReadOnlySpan<byte> value)
    {
        _buffer.Write(value);
        return this;
    }

    /// <summary>
    /// Appends a string as a one-byte length followed by its bytes; throws <see cref="ArgumentException"/> when it needs more than 255 bytes.
    /// </summary>
    /// <param name="value">The text to write.</param>
    /// <returns>This writer.</returns>
    public PacketWriter PStr8(string value)
    {
        var bytes = GameEncoding.GetBytes(value);
        if (bytes.Length > byte.MaxValue)
            throw new ArgumentException($"String of {bytes.Length} bytes does not fit a u8 length.", nameof(value));
        U8((byte)bytes.Length);
        return Bytes(bytes);
    }

    /// <summary>Appends a string's bytes without any length prefix.</summary>
    /// <param name="value">The text to write.</param>
    /// <returns>This writer.</returns>
    public PacketWriter RawString(string value) => Bytes(GameEncoding.GetBytes(value));

    /// <summary>Copies the bytes written so far.</summary>
    /// <returns>The packet bytes.</returns>
    public byte[] ToArray() => _buffer.WrittenSpan.ToArray();
}
