using System.Collections.Immutable;
using Microsoft.Extensions.Logging;
using NorthIsland.Protocol.IO;
using NorthIsland.Protocol.Messages.Items;
using NorthIsland.Protocol.Opcodes;

namespace NorthIsland.Server.Features.Mall;

/// <summary>Handles packing a piece of equipment into a Present Case and opening a full case again.</summary>
/// <param name="items">Item definitions.</param>
/// <param name="logger">Logger.</param>
public sealed class PresentCaseHandlers(
    ItemData items,
    ILogger<PresentCaseHandlers> logger)
{
    /// <summary>Item ids of the empty and of the full Present Case.</summary>
    public const ushort EmptyCase = 34200, FullCase = 34194; // 34200 Present Case, 34194 Present Case

    /// <summary>Shop picture ids of the equipment a case takes.</summary>
    public static readonly ImmutableArray<ushort> ShopPictures =
        [19681, 19682, 20301, 20302, 19891, 19891, 19651, 20807, 19601, 13822, 13901, 13831, 19571, 19561, 19561, 28841, 21871, 29101, 20321, 20808];

    /// <summary>Whether an item is equipment (slots 1 to 6) of a kind a case takes.</summary>
    /// <param name="itemId">The item to check.</param>
    /// <returns>True when it can be packed.</returns>
    public bool CanPack(ushort itemId) =>
        items[itemId] is { } r && r.EquipSlot is >= 1 and <= 6 && r.Words26.Length > 0 && ShopPictures.IndexOf(r.Words26[0]) >= 0;

    /// <summary>
    /// Packs a bag item into an empty case or opens a full case, as the request's option says (1 pack, 2 open), and answers with a success result.
    /// </summary>
    /// <param name="session">The player's session.</param>
    /// <param name="body">The packet body after the action and sub code.</param>
    internal ValueTask OnPacking(GameSession session, ReadOnlyMemory<byte> body)
    {
        var c = session.ActiveCharacter;
        if (session.State != SessionState.InWorld || c is null || session.Battle is not null || body.Length < 3)
            return ValueTask.CompletedTask;
        var (option, boxSlot, itemSlot) = (body.Span[0], body.Span[1], body.Span[2]);
        var box = c.Bag.Find(b => b.Slot == boxSlot);
        if (box is null)
            return ValueTask.CompletedTask;
        if (option == 1
            && box.ItemId == EmptyCase
            && c.Bag.Find(b => b.Slot == itemSlot) is { } piece
            && piece != box
            && CanPack(piece.ItemId)
            && piece.Pose == 0
            && session.Vehicle?.BagSlot != itemSlot)
        {
            c.Bag.Remove(piece);
            session.Send(ItemMessages.BagSlotDecrease(piece.Slot, 1));
            Replace(session, c, box, FullCase, piece.ItemId, piece.Wear, piece.Insert, piece.Fusion);
            session.Send(new PacketWriter(Action23.Code, Action23.PackingResult).U8(1).ToArray());
            logger.LogInformation("[{Session}] {Name} packs {Item} into a Present Case", session.Id, c.Name, items[piece.ItemId]?.Name.Trim());
        }
        else if (option == 2 && box.ItemId == FullCase && box.BoxItem != 0 && CanPack(box.BoxItem))
        {
            var packed = box.BoxItem;
            var (wear, insert, fusion) = (box.Wear, box.Insert, box.Fusion);
            Replace(session, c, box, packed, 0, wear, insert, fusion);
            session.Send(new PacketWriter(Action23.Code, Action23.PackingResult).U8(1).ToArray());
            logger.LogInformation("[{Session}] {Name} opens a Present Case: {Item}", session.Id, c.Name, items[packed]?.Name.Trim());
        }
        return ValueTask.CompletedTask;
    }

    /// <summary>Replaces the case in a bag slot with another item and sends the bag changes.</summary>
    /// <param name="session">The player's session.</param>
    /// <param name="c">The character.</param>
    /// <param name="old">The item now in the slot.</param>
    /// <param name="newId">Item id of the replacement.</param>
    /// <param name="boxItem">The packed item id carried by the replacement; 0 for none.</param>
    /// <param name="wear">Wear value the replacement gets.</param>
    /// <param name="insert">Inserted stone id the replacement gets.</param>
    /// <param name="fusion">Fusion level the replacement gets.</param>
    private static void Replace(
        GameSession session,
        Character c,
        BagItem old,
        ushort newId,
        ushort boxItem,
        byte wear,
        ushort insert,
        byte fusion)
    {
        c.Bag.Remove(old);
        session.Send(ItemMessages.BagSlotDecrease(old.Slot, 1));
        var made = new BagItem { Slot = old.Slot, ItemId = newId, Count = 1, Wear = wear, Insert = insert, Fusion = fusion, BoxItem = boxItem };
        c.Bag.Add(made);
        session.Send(
            ItemMessages.BagAdd(
                new BagEntry { Slot = made.Slot, ItemId = newId, Count = 1, Wear = wear, Insert = insert, FusionLevel = fusion, BoxItemId = boxItem }));
    }
}
