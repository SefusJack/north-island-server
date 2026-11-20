using Microsoft.Extensions.Logging;
using NorthIsland.Protocol.Messages.Items;

namespace NorthIsland.Server.Features.Storage;

/// <summary>
/// The city warehouse: 50 plain slots of items stored per character. Its window is not opened (an NPC's warehouse service opens the Item Bag), so
/// the deposit and withdraw requests of the window (<c>23/34</c>, <c>23/35</c>) are not served.
/// </summary>
/// <param name="logger">The logger.</param>
public sealed class WarehouseHandlers(
    ILogger<WarehouseHandlers> logger)
{
    /// <summary>The number of warehouse slots (50).</summary>
    public const int Slots = 50;

    /// <summary>Opens the warehouse: clears every slot on the client, lists the stored stacks and shows the window.</summary>
    /// <param name="session">The player's session.</param>
    public void Open(GameSession session)
    {
        var character = session.ActiveCharacter!;
        for (var slot = 1; slot <= Slots; slot++)
            session.Send(WarehouseMessages.SlotClear((byte)slot));
        if (character.Warehouse.Count > 0)
            session.Send(WarehouseMessages.List(character.Warehouse.OrderBy(w => w.Slot).Select(Entry)));
        session.Send(WarehouseMessages.Open());
        logger.LogInformation("[{Session}] {Name}: warehouse ({Count} stack(s))", session.Id, character.Name, character.Warehouse.Count);
    }

    /// <summary>Converts a stored stack to the entry of a warehouse packet.</summary>
    /// <param name="b">The stored stack.</param>
    /// <returns>The packet entry.</returns>
    private static WareEntry Entry(BagItem b) => new(b.Slot, b.ItemId, b.Count, b.Wear);
}
