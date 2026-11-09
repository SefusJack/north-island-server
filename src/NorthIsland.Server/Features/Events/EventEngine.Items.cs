using Microsoft.Extensions.Logging;
using NorthIsland.Protocol.Messages.Items;

namespace NorthIsland.Server.Features.Events;

public sealed partial class EventEngine
{
    /// <summary>Gives the character the items of an item result, or takes them when the count is negative, and tells the client.</summary>
    /// <param name="session">The player's session.</param>
    /// <param name="character">The character.</param>
    /// <param name="run">The running event, for the log.</param>
    /// <param name="mapped">The mapped result: the item id is the wire target and the count the wire value.</param>
    /// <returns>The number of items given; 0 for a take or an unknown item.</returns>
    private int ApplyItem(GameSession session, Character character, EventRun run, MappedResult mapped)
    {
        var itemId = mapped.Wire.Target;
        var count = mapped.Wire.Value;
        if (items[itemId] is null)
        {
            logger.LogWarning("[{Session}] {Source}: item {Item} is not in Item.dat, skipped", session.Id, run.Source, itemId);
            return 0;
        }
        if (count < 0)
        {
            var removed = Bag.Remove(character, itemId, -count);
            for (var left = removed; left > 0; left -= byte.MaxValue)
                session.Send(ItemMessages.BagRemoveById(itemId, (byte)Math.Min(left, byte.MaxValue)));
            logger.LogInformation("[{Session}] {Source}: {What} (took {Removed}, {Left} left)", session.Id, run.Source,
                mapped.Description, removed, Bag.Count(character, itemId));
            return 0;
        }

        var changes = Bag.Add(character, items, itemId, count);
        foreach (var (stack, added) in changes)
            session.Send(ItemMessages.BagAdd(new BagEntry { Slot = stack.Slot, ItemId = itemId, Count = added, Wear = stack.Wear }));
        var given = changes.Sum(c => c.Added);
        logger.LogInformation("[{Session}] {Source}: {What} ({Name}) into slot(s) {Slots}{Full}", session.Id, run.Source, mapped.Description,
            items[itemId]!.Name, string.Join(',', changes.Select(c => c.Item.Slot).Distinct()), given < count ? $", bag full: {count - given} lost" : "");
        return given;
    }
}
