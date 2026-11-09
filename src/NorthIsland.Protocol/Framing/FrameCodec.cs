namespace NorthIsland.Protocol.Framing;

/// <summary>Builds the frames of the wire format: a 4-byte header followed by the payload, all XORed with a fixed key.</summary>
public static class FrameCodec
{
    /// <summary>The byte every byte of a frame is XORed with.</summary>
    public const byte XorKey = 0xAD;

    /// <summary>The first byte of a frame header.</summary>
    public const byte Magic0 = 0xF4;

    /// <summary>The second byte of a frame header.</summary>
    public const byte Magic1 = 0x44;

    /// <summary>The size in bytes of the header: two magic bytes and the u16 payload length.</summary>
    public const int HeaderSize = 4;

    /// <summary>The largest payload that fits the u16 length.</summary>
    public const int MaxPayload = ushort.MaxValue;

    /// <summary>Builds a frame from a payload.</summary>
    /// <param name="payload">The payload.</param>
    /// <returns>The XORed frame: magic bytes, u16 little-endian length and the payload.</returns>
    public static byte[] Encode(ReadOnlySpan<byte> payload)
    {
        if (payload.Length > MaxPayload)
            throw new ArgumentException($"Payload of {payload.Length} bytes does not fit a u16 length.", nameof(payload));

        var frame = new byte[HeaderSize + payload.Length];
        frame[0] = Magic0;
        frame[1] = Magic1;
        frame[2] = (byte)payload.Length;
        frame[3] = (byte)(payload.Length >> 8);
        payload.CopyTo(frame.AsSpan(HeaderSize));
        Xor(frame);
        return frame;
    }

    /// <summary>XORs every byte with the key, in place; the same call encodes and decodes.</summary>
    /// <param name="data">The bytes to convert.</param>
    public static void Xor(Span<byte> data)
    {
        for (var i = 0; i < data.Length; i++)
            data[i] ^= XorKey;
    }
}
