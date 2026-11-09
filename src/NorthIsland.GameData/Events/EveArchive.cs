namespace NorthIsland.GameData.Events;

/// <summary>The parsed <c>eve.Emg</c> file: an index of per-map event scene blobs plus the convey name table.</summary>
public sealed class EveArchive
{
    /// <summary>Map id whose scene is used when a requested map has no entry in the index.</summary>
    public const int FallbackMapId = 12003; // Civillian's House

    /// <summary>Size in bytes of the file header (timestamp and scene count).</summary>
    private const int HeaderSize = 12;

    /// <summary>Size in bytes of one index entry.</summary>
    private const int IndexEntrySize = 10;

    /// <summary>The whole file; scene blobs are slices of it.</summary>
    private readonly byte[] _data;

    /// <summary>Index entries by map id; the first entry wins when a map id repeats.</summary>
    private readonly Dictionary<int, EveIndexEntry> _byMapId = [];

    /// <summary>Creates the archive over already parsed parts.</summary>
    /// <param name="data">The whole file.</param>
    /// <param name="timestamp">The header timestamp.</param>
    /// <param name="entries">The index entries in file order.</param>
    /// <param name="conveyNames">The convey names after the last scene.</param>
    private EveArchive(
        byte[] data,
        double timestamp,
        List<EveIndexEntry> entries,
        List<EveConveyName> conveyNames)
    {
        _data = data;
        Timestamp = timestamp;
        Entries = entries;
        ConveyNames = conveyNames;
        foreach (var e in entries)
            _byMapId.TryAdd(e.MapId, e);
    }

    /// <summary>The header timestamp, an OLE automation date (days since 1899-12-30).</summary>
    public double Timestamp { get; }

    /// <summary>The timestamp as a date, or null when it is not a plausible OLE date.</summary>
    public DateTime? BuildTime => Timestamp is > 1 and < 2958465 ? DateTime.FromOADate(Timestamp) : null;

    /// <summary>The scene index in file order.</summary>
    public IReadOnlyList<EveIndexEntry> Entries { get; }

    /// <summary>The convey names stored after the last scene.</summary>
    public IReadOnlyList<EveConveyName> ConveyNames { get; }

    /// <summary>The map ids that have a scene, in index order.</summary>
    public IEnumerable<int> MapIds => Entries.Select(e => (int)e.MapId);

    /// <summary>Reads and parses an archive file.</summary>
    /// <param name="path">Path of the <c>eve.Emg</c> file.</param>
    /// <returns>The parsed archive.</returns>
    public static EveArchive Open(string path) => Parse(File.ReadAllBytes(path));

    /// <summary>Parses an archive from its bytes, checking that the index and every scene lie inside the data.</summary>
    /// <param name="data">The file contents.</param>
    /// <returns>The parsed archive.</returns>
    public static EveArchive Parse(byte[] data)
    {
        ArgumentNullException.ThrowIfNull(data);
        var r = new EveReader(data);
        var timestamp = r.F64();
        var count = r.I32();
        if (count <= 0 || HeaderSize + (long)count * IndexEntrySize > data.Length)
            throw new EveFormatException($"Implausible scene count {count}.");

        var entries = new List<EveIndexEntry>(count);
        for (var i = 0; i < count; i++)
        {
            var entry = new EveIndexEntry(r.U16(), r.U16(), r.I32(), r.U16());
            if (entry.Offset < 0 || (long)entry.Offset + entry.Size > data.Length)
                throw new EveFormatException($"Scene {entry.MapId} ({entry.Offset}+{entry.Size}) lies outside the file.");
            entries.Add(entry);
        }

        var last = entries[^1];
        var conveyNames = ReadConveyNames(data.AsSpan(last.Offset + last.Size));
        return new EveArchive(data, timestamp, entries, conveyNames);
    }

    /// <summary>Reads the convey name table that follows the last scene.</summary>
    /// <param name="tail">The bytes after the last scene.</param>
    /// <returns>The names; empty when the tail is too short.</returns>
    private static List<EveConveyName> ReadConveyNames(ReadOnlySpan<byte> tail)
    {
        var names = new List<EveConveyName>();
        if (tail.Length < 2)
            return names;
        var r = new EveReader(tail);
        int count = r.U16();
        for (var i = 0; i < count; i++)
            names.Add(new EveConveyName(r.U16(), r.U8(), r.ShortString(21)));
        return names;
    }

    /// <summary>Tells whether the index has a scene for a map.</summary>
    /// <param name="mapId">The map id.</param>
    /// <returns>True when the map has a scene.</returns>
    public bool Contains(int mapId) => _byMapId.ContainsKey(mapId);

    /// <summary>Returns the raw scene blob of a map.</summary>
    /// <param name="mapId">The map id.</param>
    /// <returns>The blob slice of the file.</returns>
    public ReadOnlyMemory<byte> GetSceneBytes(int mapId)
    {
        if (!_byMapId.TryGetValue(mapId, out var entry))
            throw new KeyNotFoundException($"Map {mapId} has no event scene.");
        return _data.AsMemory(entry.Offset, entry.Size);
    }

    /// <summary>Decodes the scene of a map.</summary>
    /// <param name="mapId">The map id.</param>
    /// <returns>The decoded scene.</returns>
    public EveScene LoadScene(int mapId) => EveSceneReader.Read(GetSceneBytes(mapId).Span);
}
