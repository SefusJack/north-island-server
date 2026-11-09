using System.Buffers.Binary;
using System.Globalization;
using System.Reflection;
using Microsoft.Extensions.Logging;
using NorthIsland.GameData.Items;
using NorthIsland.Protocol.Messages.Items;

namespace NorthIsland.Server.Features.Items;

/// <summary>The Magnifying Glass search: shows the material codes, rank and required level of an item in the bag.</summary>
/// <param name="items">Item definitions and recipes.</param>
/// <param name="logger">Logger.</param>
public sealed class ReferHandlers(
    ItemData items,
    ILogger<ReferHandlers> logger)
{
    /// <summary>The item id of the Magnifying Glass.</summary>
    public const ushort MagnifyingGlass = 34179; // Magnifying Glass

    /// <summary>The mall points a search costs when it is paid with points instead of a glass.</summary>
    public const int PointCost = 2;

    /// <summary>The material code of each icon, read from an embedded table.</summary>
    private static readonly Lazy<IReadOnlyDictionary<ushort, byte>> CodeByIcon = new(LoadCodes);

    /// <summary>Reads the embedded table of material codes by icon.</summary>
    /// <returns>The codes by icon id; empty when the table is missing.</returns>
    private static Dictionary<ushort, byte> LoadCodes()
    {
        var map = new Dictionary<ushort, byte>();
        using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("NorthIsland.Server.Data.MaterialCodes.tsv");
        if (stream is null)
            return map;
        using var reader = new StreamReader(stream);
        while (reader.ReadLine() is { } line)
        {
            var parts = line.Split('\t');
            if (parts.Length == 3
                && byte.TryParse(parts[0], CultureInfo.InvariantCulture, out var code)
                && ushort.TryParse(parts[2], CultureInfo.InvariantCulture, out var icon))
                map.TryAdd(icon, code);
        }
        return map;
    }

    /// <summary>The material codes of an item: those of its recipe's materials, or else its alchemy classes.</summary>
    /// <param name="item">The item definition.</param>
    /// <returns>Five codes; unused places are 0.</returns>
    public byte[] Codes(ItemRecord item)
    {
        var codes = new byte[5];
        var n = 0;
        if (items.Recipes.FirstOrDefault(r => r.Product == item.Id) is { } recipe)
            foreach (var (material, _) in recipe.Materials)
                if (n < 5 && items[material] is { } m && CodeByIcon.Value.TryGetValue(m.IconId, out var code) && !codes.Contains(code))
                    codes[n++] = code;
        for (var i = 0; n == 0 && i < item.Words142.Length && n < 5; i++)
            if (Alchemy.Class(item, i) is > 0 and <= 68 and var c)
                codes[n++] = (byte)c;
        return codes;
    }

    /// <summary>
    /// Handles a search: checks the item in the bag, takes a Magnifying Glass or the mall points, and sends the item's rank, required level and codes.
    /// </summary>
    /// <param name="session">The player's session.</param>
    /// <param name="body">The packet body: how it is paid (1 glass, 2 points), the item id, the bag slot of the item and the bag slot of the glass.</param>
    internal ValueTask OnSearch(GameSession session, ReadOnlyMemory<byte> body)
    {
        if (session.State != SessionState.InWorld || session.ActiveCharacter is not { } c || session.Account is null || body.Length < 5)
            return ValueTask.CompletedTask;
        var kind = body.Span[0];
        var itemId = BinaryPrimitives.ReadUInt16LittleEndian(body.Span[1..]);
        var (slot, glassSlot) = (body.Span[3], body.Span[4]);
        var stack = c.Bag.Find(b => b.Slot == slot);
        if (stack is null || stack.ItemId != itemId || items[itemId] is not { } item || Codes(item)[0] == 0)
        {
            session.Send(ReferMessages.Refused());
            return ValueTask.CompletedTask;
        }
        if (kind == 1)
        {
            var glass = c.Bag.Find(b => b.Slot == glassSlot && b.ItemId == MagnifyingGlass);
            if (glass is null)
                return ValueTask.CompletedTask;
            glass.Count--;
            session.Send(ItemMessages.BagSlotDecrease(glass.Slot, 1));
            if (glass.Count == 0)
                c.Bag.Remove(glass);
        }
        else if (kind == 2)
        {
            if (session.Account.MallPoints < PointCost)
                return ValueTask.CompletedTask;
            session.Account.MallPoints -= PointCost;
            session.Send(MallMessages.Points(session.Account.MallPoints));
        }
        else
            return ValueTask.CompletedTask;
        session.Send(ReferMessages.Result((byte)Math.Min(255, (int)item.ShopColumn), (byte)Math.Min(255, (int)item.RequiredLevel), Codes(item)));
        logger.LogInformation("[{Session}] {Name} searches {Item}", session.Id, c.Name, item.Name);
        return ValueTask.CompletedTask;
    }
}
