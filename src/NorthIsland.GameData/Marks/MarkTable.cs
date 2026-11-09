using System.Buffers.Binary;
using NorthIsland.GameData.Talk;

namespace NorthIsland.GameData.Marks;

/// <summary>The marks of Mark.Dat, looked up by id or completion flag bit.</summary>
public sealed class MarkTable
{
    /// <summary>Size of one Mark.Dat record in bytes.</summary>
    public const int RecordSize = 553;

    /// <summary>Flag bit the header record must carry.</summary>
    public const ushort HeaderFlagBit = 4;

    /// <summary>The marks by id.</summary>
    private readonly Dictionary<ushort, MarkRecord> _byId;

    /// <summary>The marks that have a flag bit, by flag bit; the first one wins when several share a bit.</summary>
    private readonly Dictionary<ushort, MarkRecord> _byFlagBit;

    /// <summary>Creates the table and indexes the marks by flag bit.</summary>
    /// <param name="byId">The marks by id.</param>
    private MarkTable(
        Dictionary<ushort, MarkRecord> byId)
    {
        _byId = byId;
        _byFlagBit = byId.Values.Where(m => m.FlagBit != 0).GroupBy(m => m.FlagBit).ToDictionary(g => g.Key, g => g.First());
    }

    /// <summary>Number of marks.</summary>
    public int Count => _byId.Count;

    /// <summary>Looks up a mark; null when the id is unknown.</summary>
    /// <param name="id">The mark id.</param>
    public MarkRecord? this[ushort id] => _byId.GetValueOrDefault(id);

    /// <summary>Looks up the mark that owns a completion flag bit.</summary>
    /// <param name="bit">The flag bit.</param>
    /// <returns>The mark, or null when no mark has the bit.</returns>
    public MarkRecord? ByFlagBit(ushort bit) => _byFlagBit.GetValueOrDefault(bit);

    /// <summary>Reads and parses a Mark.Dat file.</summary>
    /// <param name="path">Path of the file.</param>
    /// <returns>The parsed table.</returns>
    public static MarkTable Open(string path) => Parse(File.ReadAllBytes(path));

    /// <summary>
    /// Parses the contents of a Mark.Dat file; throws <see cref="InvalidDataException"/> when the header lacks the expected
    /// flag bit.
    /// </summary>
    /// <param name="data">The raw file contents.</param>
    /// <returns>The parsed table.</returns>
    public static MarkTable Parse(ReadOnlySpan<byte> data)
    {
        if (data.Length < RecordSize || BinaryPrimitives.ReadUInt16LittleEndian(data[258..]) != HeaderFlagBit)
            throw new InvalidDataException("Mark.Dat header record does not carry flag bit 4");
        var byId = new Dictionary<ushort, MarkRecord>();
        for (var offset = RecordSize; offset + RecordSize <= data.Length; offset += RecordSize)
        {
            var r = data.Slice(offset, RecordSize);
            var id = U16(r, 256);
            if (id == 0)
                continue;
            byId.TryAdd(id, new MarkRecord(id,
                U16(r, 258), // 258 FlagBit
                TalkTable.DecodeText(r[..255]), // 255 Name
                U8(r, 517) != 0, // 517 HiddenInList
                U8(r, 516))); // 516 Stars
        }
        return new MarkTable(byId);
    }

    /// <summary>Decodes an obfuscated little-endian word.</summary>
    /// <param name="r">The record.</param>
    /// <param name="at">Byte offset in the record.</param>
    /// <returns>The decoded word.</returns>
    private static ushort U16(ReadOnlySpan<byte> r, int at) => (ushort)((BinaryPrimitives.ReadUInt16LittleEndian(r[at..]) ^ 10100) - 7);

    /// <summary>Decodes an obfuscated byte.</summary>
    /// <param name="r">The record.</param>
    /// <param name="at">Byte offset in the record.</param>
    /// <returns>The decoded byte.</returns>
    private static byte U8(ReadOnlySpan<byte> r, int at) => (byte)((r[at] ^ 144) - 7);
}
