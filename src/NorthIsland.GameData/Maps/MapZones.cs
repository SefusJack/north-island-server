using System.Buffers.Binary;
using System.Text;

namespace NorthIsland.GameData.Maps;

/// <summary>The battle zones of the maps in Ground.MMG, read from the archive when asked.</summary>
public sealed class MapZones
{
    /// <summary>Size of one archive index record in bytes.</summary>
    private const int IndexRecordSize = 29;

    /// <summary>Path of the archive.</summary>
    private readonly string _path;

    /// <summary>The archive offset and size of each map entry, by map id.</summary>
    private readonly Dictionary<int, (int Offset, int Size)> _index = [];

    /// <summary>Creates the reader.</summary>
    /// <param name="path">Path of the archive.</param>
    private MapZones(
        string path) => _path = path;

    /// <summary>Opens the archive and reads its index.</summary>
    /// <param name="path">Path of the archive.</param>
    /// <returns>The reader.</returns>
    public static MapZones Open(string path)
    {
        var zones = new MapZones(path);
        using var stream = File.OpenRead(path);
        Span<byte> countBytes = stackalloc byte[2];
        stream.Seek(-2, SeekOrigin.End);
        stream.ReadExactly(countBytes);
        var count = BinaryPrimitives.ReadUInt16LittleEndian(countBytes);
        var table = new byte[count * IndexRecordSize];
        stream.Seek(-(table.Length + 2), SeekOrigin.End);
        stream.ReadExactly(table);
        for (var i = 0; i < count; i++)
        {
            var rec = table.AsSpan(i * IndexRecordSize, IndexRecordSize);
            var name = Encoding.Latin1.GetString(rec.Slice(1, Math.Min((int)rec[0], 20)));
            var dot = name.IndexOf('.', StringComparison.Ordinal);
            if (int.TryParse(dot < 0 ? name : name[..dot], out var mapId))
                zones._index.TryAdd(
                    mapId,
                    (BinaryPrimitives.ReadInt32LittleEndian(rec.Slice(21, 4)), // 21 Offset
                        BinaryPrimitives.ReadInt32LittleEndian(rec.Slice(25, 4)))); // 25 Size
        }
        return zones;
    }

    /// <summary>Number of maps in the archive.</summary>
    public int MapCount => _index.Count;

    /// <summary>The ids of the maps in the archive.</summary>
    public IEnumerable<int> MapIds => _index.Keys;

    /// <summary>Reads the battle zones of a map.</summary>
    /// <param name="mapId">The map id.</param>
    /// <returns>The zones; empty when the map is not in the archive.</returns>
    public IReadOnlyList<MapZone> For(int mapId)
    {
        if (!_index.TryGetValue(mapId, out var entry))
            return [];
        var data = new byte[entry.Size];
        using (var stream = File.OpenRead(_path))
        {
            stream.Seek(entry.Offset, SeekOrigin.Begin);
            stream.ReadExactly(data);
        }
        return Parse(data);
    }

    /// <summary>Reads the size of a map.</summary>
    /// <param name="mapId">The map id.</param>
    /// <returns>Width and height in pixels, or null when the map is unknown.</returns>
    public (int Width, int Height)? Size(int mapId)
    {
        if (!_index.TryGetValue(mapId, out var entry) || entry.Size < 8)
            return null;
        Span<byte> head = stackalloc byte[8];
        using var stream = File.OpenRead(_path);
        stream.Seek(entry.Offset, SeekOrigin.Begin);
        stream.ReadExactly(head);
        return (BinaryPrimitives.ReadInt32LittleEndian(head), BinaryPrimitives.ReadInt32LittleEndian(head[4..]));
    }

    /// <summary>Parses the battle zones out of the data of one map.</summary>
    /// <param name="d">The data of the map entry.</param>
    /// <returns>The zones.</returns>
    public static IReadOnlyList<MapZone> Parse(ReadOnlySpan<byte> d)
    {
        var p = 8;
        p += 1 + d[p] * 6;
        var aw = BinaryPrimitives.ReadUInt16LittleEndian(d[p..]);
        var ah = BinaryPrimitives.ReadUInt16LittleEndian(d[(p + 2)..]);
        p += 4 + aw * ah;
        p += 2 + BinaryPrimitives.ReadUInt16LittleEndian(d[p..]) * 6;
        p += 2 + BinaryPrimitives.ReadUInt16LittleEndian(d[p..]) * 8;
        p += 2;
        var count = d[p++];
        var zones = new List<MapZone>(count);
        for (var i = 0; i < count; i++, p += 9)
        {
            zones.Add(new MapZone(d[p],
                BinaryPrimitives.ReadUInt16LittleEndian(d[(p + 1)..]), BinaryPrimitives.ReadUInt16LittleEndian(d[(p + 3)..]), // 1 X1, 3 Y1
                BinaryPrimitives.ReadUInt16LittleEndian(d[(p + 5)..]), BinaryPrimitives.ReadUInt16LittleEndian(d[(p + 7)..]))); // 5 X2, 7 Y2
        }
        return zones;
    }
}
