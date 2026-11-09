using System.Text;

namespace NorthIsland.Protocol.Text;

/// <summary>The text encoding of the packets.</summary>
public static class GameEncoding
{
    /// <summary>Registers the code page provider and selects code page 1252 as the packet encoding.</summary>
    static GameEncoding()
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        Current = Encoding.GetEncoding(1252);
    }

    /// <summary>The encoding (code page 1252) used for every string in a packet.</summary>
    public static Encoding Current { get; }

    /// <summary>Decodes packet bytes to text.</summary>
    /// <param name="bytes">The encoded text.</param>
    /// <returns>The decoded text.</returns>
    public static string GetString(ReadOnlySpan<byte> bytes) => Current.GetString(bytes);

    /// <summary>Encodes text for a packet.</summary>
    /// <param name="text">The text to encode.</param>
    /// <returns>The encoded bytes.</returns>
    public static byte[] GetBytes(string text) => Current.GetBytes(text);
}
