using System.Globalization;
using System.Reflection;

namespace NorthIsland.Server.Features.Mall;

/// <summary>The reward tables of the mall boxes and lucky bags.</summary>
public static class MallBoxes
{

    /// <summary>Reads the embedded box tables and applies the optional lucky bag override file.</summary>
    /// <param name="luckyBagsFile">Path of the JSON override file, or null or empty for none.</param>
    /// <returns>The reward rows by box item id.</returns>
    public static IReadOnlyDictionary<ushort, IReadOnlyList<MallBoxEntry>> Load(string? luckyBagsFile = null)
    {
        var result = new Dictionary<ushort, List<MallBoxEntry>>();
        foreach (var name in new[] { "MallBoxes.tsv", "LuckyBags.tsv", "BoxGuesses.tsv" })
        {
            using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("NorthIsland.Server.Data." + name);
            if (stream is null)
                continue;
            using var reader = new StreamReader(stream);
            while (reader.ReadLine() is { } line)
            {
                var parts = line.Trim().Split((char)9);
                if (parts.Length != 4 || !ushort.TryParse(parts[0], out var box))
                    continue;
                if (!result.TryGetValue(box, out var rows))
                    result[box] = rows = [];
                rows.Add(
                    new MallBoxEntry(
                        ushort.Parse(parts[1], CultureInfo.InvariantCulture),
                        byte.Parse(parts[2], CultureInfo.InvariantCulture),
                        int.Parse(parts[3], CultureInfo.InvariantCulture)));
            }
        }
        if (!string.IsNullOrWhiteSpace(luckyBagsFile) && File.Exists(luckyBagsFile))
        {
            var edited = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, List<JsonRow>>>(File.ReadAllText(luckyBagsFile)) ?? [];
            foreach (var (key, rows) in edited)
                if (ushort.TryParse(key, out var bag))
                    result[bag] = [.. rows.Select(r => new MallBoxEntry(r.Item, r.Count == 0 ? (byte)1 : r.Count, r.Weight == 0 ? 1 : r.Weight))];
        }
        return result.ToDictionary(kv => kv.Key, kv => (IReadOnlyList<MallBoxEntry>)kv.Value);
    }

    /// <summary>One row of the lucky bag JSON file.</summary>
    /// <param name="Item">The item id.</param>
    /// <param name="Count">How many are given; 0 counts as 1.</param>
    /// <param name="Weight">Relative chance; 0 counts as 1.</param>
    private sealed record JsonRow(
        ushort Item,
        byte Count,
        int Weight);

    /// <summary>Picks one entry of a table, weighted by the entries' weights.</summary>
    /// <param name="table">The box's reward rows.</param>
    /// <param name="roll">Returns a number from 0 up to, not including, the total weight it is given.</param>
    /// <returns>The chosen entry.</returns>
    public static MallBoxEntry Roll(IReadOnlyList<MallBoxEntry> table, Func<int, int> roll)
    {
        var pick = roll(table.Sum(e => e.Weight));
        foreach (var e in table)
        {
            if (pick < e.Weight)
                return e;
            pick -= e.Weight;
        }
        return table[^1];
    }
}
