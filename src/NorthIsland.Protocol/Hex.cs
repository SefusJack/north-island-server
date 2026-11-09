namespace NorthIsland.Protocol;

/// <summary>Hex helpers for packet bytes, used in logs and tests.</summary>
public static class Hex
{
    /// <summary>Formats bytes as upper-case hex pairs separated by single spaces.</summary>
    /// <param name="data">The bytes to format.</param>
    /// <returns>The hex text, empty when there are no bytes.</returns>
    public static string Format(ReadOnlySpan<byte> data) => Convert.ToHexString(data) switch
    {
        "" => "", // no bytes: empty text
        var s => string.Join(' ', Enumerable.Range(0, s.Length / 2).Select(i => s.Substring(i * 2, 2))), // any bytes: hex pairs joined by single spaces
    };

    /// <summary>Reads hex text back into bytes; spaces and dashes between the digits are ignored.</summary>
    /// <param name="text">Hex digits, optionally separated by spaces or dashes.</param>
    /// <returns>The decoded bytes.</returns>
    public static byte[] Parse(string text) =>
        Convert.FromHexString(text.Replace(" ", "", StringComparison.Ordinal).Replace("-", "", StringComparison.Ordinal));
}
