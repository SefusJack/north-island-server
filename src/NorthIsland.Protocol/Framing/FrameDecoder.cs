namespace NorthIsland.Protocol.Framing;

/// <summary>Collects the bytes read from a socket and cuts them into frame payloads.</summary>
public sealed class FrameDecoder
{
    /// <summary>The decoded bytes not yet consumed.</summary>
    private byte[] _buffer = new byte[4096];

    /// <summary>The number of valid bytes in the buffer.</summary>
    private int _count;

    /// <summary>The number of bytes dropped while looking for a frame header.</summary>
    public long SkippedBytes { get; private set; }

    /// <summary>Appends bytes read from the socket, decoding them.</summary>
    /// <param name="raw">The bytes as received.</param>
    public void Feed(ReadOnlySpan<byte> raw)
    {
        EnsureCapacity(_count + raw.Length);
        var dest = _buffer.AsSpan(_count, raw.Length);
        raw.CopyTo(dest);
        FrameCodec.Xor(dest);
        _count += raw.Length;
    }

    /// <summary>Takes the next complete frame's payload out of the buffer, dropping two bytes at a time while the header is not valid.</summary>
    /// <returns>The payload, or null when no complete frame is buffered.</returns>
    public byte[]? NextPayload()
    {
        while (_count >= FrameCodec.HeaderSize)
        {
            if (_buffer[0] != FrameCodec.Magic0 || _buffer[1] != FrameCodec.Magic1)
            {
                Consume(2);
                SkippedBytes += 2;
                continue;
            }

            var length = _buffer[2] | (_buffer[3] << 8);
            if (_count - FrameCodec.HeaderSize < length)
                break;

            var payload = _buffer.AsSpan(FrameCodec.HeaderSize, length).ToArray();
            Consume(FrameCodec.HeaderSize + length);
            return payload;
        }

        return null;
    }

    /// <summary>Removes the first bytes from the buffer.</summary>
    /// <param name="n">The number of bytes.</param>
    private void Consume(int n)
    {
        _count -= n;
        _buffer.AsSpan(n, _count).CopyTo(_buffer);
    }

    /// <summary>Grows the buffer so that it holds at least a number of bytes.</summary>
    /// <param name="needed">The number of bytes needed.</param>
    private void EnsureCapacity(int needed)
    {
        if (needed <= _buffer.Length)
            return;
        Array.Resize(ref _buffer, Math.Max(needed, _buffer.Length * 2));
    }
}
