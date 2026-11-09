using Microsoft.Extensions.Logging;
using NorthIsland.Protocol.IO;
using NorthIsland.Protocol.Messages.Items;

namespace NorthIsland.Server.Features.Items;

/// <summary>Repairs: a Repairing Spanner or Potion resets the wear of a piece of gear in the bag.</summary>
/// <param name="items">Item definitions.</param>
/// <param name="logger">Logger.</param>
public sealed class RepairHandlers(
    ItemData items,
    ILogger<RepairHandlers> logger)
{
    /// <summary>The item id of the Repairing Spanner.</summary>
    public const ushort Spanner = 34104; // Repairing Spanner

    /// <summary>The item id of the Repairing Potion.</summary>
    public const ushort Potion = 34105; // Repairing Potion

    /// <summary>Handles a repair: refuses gear that cannot be repaired, resets its wear and uses up the tool.</summary>
    /// <param name="session">The player's session.</param>
    /// <param name="body">The packet body: the item id of the gear, the bag slot of the gear and the bag slot of the tool.</param>
    internal ValueTask OnRepair(GameSession session, ReadOnlyMemory<byte> body)
    {
        if (session.State != SessionState.InWorld || session.ActiveCharacter is not { } c || body.Length < 4 || session.Battle is not null)
            return ValueTask.CompletedTask;
        var r = new PacketReader(body.Span);
        var (itemId, gearSlot, toolSlot) = (r.U16(), r.U8(), r.U8());
        var gear = c.Bag.Find(b => b.Slot == gearSlot);
        var tool = c.Bag.Find(b => b.Slot == toolSlot);
        var record = gear is null ? null : items[gear.ItemId];
        if (gear is null || tool is null || gear.ItemId != itemId || tool.ItemId is not (Spanner or Potion) || record is null
            || record.Type is < 1 or > 16 || record.EquipSlot is < 1 or > 6 || record.Words429.FirstOrDefault() != 0
            || record.SubType == 149 // BlessGearSubType
            || Durability.IsDestroyed(gear.Wear) || gear.Wear == 1)
        {
            logger.LogInformation("[{Session}] {Name}: repair refused (gear slot {Gear}, tool slot {Tool})", session.Id, c.Name, gearSlot, toolSlot);
            return ValueTask.CompletedTask;
        }
        if (tool.ItemId == Spanner && gear.Wear >= Durability.SpannerMaxWear)
        {
            logger.LogInformation("[{Session}] {Name}: the Spanner cannot mend {Item} at wear {Wear}", session.Id, c.Name, record.Name.Trim(), gear.Wear);
            return ValueTask.CompletedTask;
        }
        gear.Wear = 1;
        session.Send(ItemMessages.BagWear(gear.Slot, gear.Wear));
        if (tool.ItemId == Spanner && items[Spanner] is { } spanner)
            UseLimit.Spend(session, c, tool, spanner);
        else
        {
            tool.Count--;
            session.Send(ItemMessages.BagSlotDecrease(tool.Slot, 1));
            if (tool.Count == 0)
                c.Bag.Remove(tool);
        }
        logger.LogInformation("[{Session}] {Name} repairs {Item}", session.Id, c.Name, record.Name.Trim());
        return ValueTask.CompletedTask;
    }
}
