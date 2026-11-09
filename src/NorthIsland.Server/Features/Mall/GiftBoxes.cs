using System.Globalization;
using System.Reflection;

namespace NorthIsland.Server.Features.Mall;

/// <summary>Loads the contents of the mall gift boxes.</summary>
public static class GiftBoxes
{

    /// <summary>
    /// Reads the embedded <c>GiftBoxes.tsv</c> and <c>GiftBoxGuesses.tsv</c> (box item id, item id or <c>gold</c>, amount) into one table.
    /// </summary>
    /// <returns>The contents by box item id.</returns>
    public static IReadOnlyDictionary<ushort, GiftBox> Load()
    {
        var items = new Dictionary<ushort, List<(ushort, int)>>();
        var gold = new Dictionary<ushort, int>();
        foreach (var file in new[] { "GiftBoxes.tsv", "GiftBoxGuesses.tsv" })
        {
            using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("NorthIsland.Server.Data." + file);
            if (stream is null)
                continue;
            using var reader = new StreamReader(stream);
            while (reader.ReadLine() is { } line)
            {
                var p = line.Trim().Split((char)9);
                if (p.Length != 3 || !ushort.TryParse(p[0], CultureInfo.InvariantCulture, out var box))
                    continue;
                if (!items.TryGetValue(box, out var list))
                    items[box] = list = [];
                if (p[1] == "gold")
                    gold[box] = int.Parse(p[2], CultureInfo.InvariantCulture);
                else
                    list.Add((ushort.Parse(p[1], CultureInfo.InvariantCulture), int.Parse(p[2], CultureInfo.InvariantCulture)));
            }
        }
        return items.ToDictionary(kv => kv.Key, kv => new GiftBox(kv.Value, gold.GetValueOrDefault(kv.Key)));
    }
}
