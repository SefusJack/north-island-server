using System.Reflection;

namespace NorthIsland.Server.Features.Crafting;

/// <summary>The embedded table that gives pieces of equipment their material classes.</summary>
public static class EquipmentClasses
{

    /// <summary>Reads the embedded <c>EquipmentClasses.tsv</c>: item id, name and comma separated class numbers on each line.</summary>
    /// <returns>The rows by item id; empty when the resource is missing.</returns>
    public static IReadOnlyDictionary<ushort, EquipmentClassEntry> Load()
    {
        var result = new Dictionary<ushort, EquipmentClassEntry>();
        using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("NorthIsland.Server.Data.EquipmentClasses.tsv");
        if (stream is null)
            return result;
        using var reader = new StreamReader(stream);
        while (reader.ReadLine() is { } line)
        {
            var parts = line.Trim().Split((char)9);
            if (parts.Length == 3 && ushort.TryParse(parts[0], out var id))
                result[id] = new EquipmentClassEntry(parts[1], [.. parts[2].Split(',').Select(ushort.Parse)]);
        }
        return result;
    }
}
