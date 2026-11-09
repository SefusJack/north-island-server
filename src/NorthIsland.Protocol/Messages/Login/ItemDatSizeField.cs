using NorthIsland.Protocol.IO;
using NorthIsland.Protocol.Text;

namespace NorthIsland.Protocol.Messages.Login;

/// <summary>Reads the item data file size that follows both login requests.</summary>
public static class ItemDatSizeField
{
    /// <summary>Reads and decodes the field.</summary>
    /// <param name="r">The reader positioned at the field.</param>
    /// <returns>The file size, or null when the field is empty or not a number.</returns>
    public static long? Read(ref PacketReader r)
    {
        var n = r.U8();
        var key = r.U8();
        var bytes = r.Bytes(n).ToArray();
        for (var i = 0; i < bytes.Length; i++)
            bytes[i] ^= key;
        return long.TryParse(GameEncoding.GetString(bytes), out var size) ? size : null;
    }
}
