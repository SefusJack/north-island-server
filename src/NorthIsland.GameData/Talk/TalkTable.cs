using System.Buffers.Binary;
using NorthIsland.GameData.Events;

namespace NorthIsland.GameData.Talk;

/// <summary>The NPC dialogue texts of Talk.Dat by id.</summary>
public sealed class TalkTable
{
    /// <summary>The size in bytes of one record.</summary>
    public const int RecordSize = 292;

    /// <summary>The raw id of the header record.</summary>
    public const ushort HeaderId = 3;

    /// <summary>The texts by id.</summary>
    private readonly Dictionary<ushort, string> _byId;

    /// <summary>Wraps the parsed texts.</summary>
    /// <param name="byId">The texts by id.</param>
    private TalkTable(
        Dictionary<ushort, string> byId) => _byId = byId;

    /// <summary>The number of texts.</summary>
    public int Count => _byId.Count;

    /// <summary>The text with an id, or null.</summary>
    /// <param name="id">The text id.</param>
    public string? this[int id] => id is >= 0 and <= ushort.MaxValue ? _byId.GetValueOrDefault((ushort)id) : null;

    /// <summary>Reads and parses a Talk.Dat file.</summary>
    /// <param name="path">Path of the file.</param>
    /// <returns>The parsed table.</returns>
    public static TalkTable Open(string path) => Parse(File.ReadAllBytes(path));

    /// <summary>Parses the contents of Talk.Dat; throws when the size or the header id is wrong.</summary>
    /// <param name="data">The whole file.</param>
    /// <returns>The parsed table.</returns>
    public static TalkTable Parse(ReadOnlySpan<byte> data)
    {
        if (data.Length < RecordSize || data.Length % RecordSize != 0)
            throw new InvalidDataException($"Talk.Dat size {data.Length} is not a multiple of {RecordSize}.");
        var headerId = BinaryPrimitives.ReadUInt16LittleEndian(data);
        if (headerId != HeaderId)
            throw new InvalidDataException($"Talk.Dat header id {headerId}, expected {HeaderId}.");

        var byId = new Dictionary<ushort, string>(data.Length / RecordSize);
        for (var offset = RecordSize; offset < data.Length; offset += RecordSize)
        {
            var record = data.Slice(offset, RecordSize);
            var id = unchecked((ushort)((BinaryPrimitives.ReadUInt16LittleEndian(record) ^ 60650) - 5));
            if (id != 0)
                byId.TryAdd(id, DecodeText(record.Slice(2, 255)));
        }
        return new TalkTable(byId);
    }

    /// <summary>Decodes a text stored as a short string with the content reversed.</summary>
    /// <param name="shortString">The 255 bytes of the string.</param>
    /// <returns>The text.</returns>
    public static string DecodeText(ReadOnlySpan<byte> shortString)
    {
        var chars = shortString.Slice(1, 254).ToArray();
        Array.Reverse(chars);
        var length = Math.Min((int)shortString[0], 254);
        return EveText.Current.GetString(chars, 0, length);
    }
}
