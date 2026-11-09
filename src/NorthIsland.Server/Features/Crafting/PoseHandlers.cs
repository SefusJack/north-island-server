using Microsoft.Extensions.Logging;
using NorthIsland.GameData.Items;
using NorthIsland.Protocol.Messages.Items;

namespace NorthIsland.Server.Features.Crafting;

/// <summary>Handles <c>23/105</c>: giving a piece of equipment the look of another (a pose) with a sewing kit, and removing it again.</summary>
/// <param name="items">The item data.</param>
/// <param name="logger">The log.</param>
public sealed class PoseHandlers(
    ItemData items,
    ILogger<PoseHandlers> logger)
{
    /// <summary>Item id of the Sewing Tools, the kit for armour pieces (equipment slots 1, 2, 4 and 5).</summary>
    public const ushort SewingTools = 34167; // Sewing Tools

    /// <summary>Item id of the Weapon Model, the kit for weapons (equipment slot 3, no guns).</summary>
    public const ushort WeaponModel = 34170; // Weapon Model

    /// <summary>Item id of the box that takes a pose off a piece again.</summary>
    public const ushort RebirthBox = 34198; // Rebirth Box

    /// <summary>Handles <c>23/105</c>: operation 1 sews a pose, operation 2 reverts one.</summary>
    /// <param name="session">The player's session.</param>
    /// <param name="body">The packet body: the operation, then that operation's bag slots.</param>
    internal ValueTask OnPose(GameSession session, ReadOnlyMemory<byte> body)
    {
        if (session.State != SessionState.InWorld || session.ActiveCharacter is not { } c || body.Length < 1)
            return ValueTask.CompletedTask;
        var op = body.Span[0];
        if (op == 1)
            Sew(session, c, body.Span[1..].ToArray());
        else if (op == 2)
            Revert(session, c, body.Span[1..].ToArray());
        return ValueTask.CompletedTask;
    }

    /// <summary>Gives the attribute piece the look of the shape piece, spends the shape piece and the kit, and tells the client.</summary>
    /// <param name="session">The player's session.</param>
    /// <param name="c">The character who sews.</param>
    /// <param name="rest">The bytes after the operation: kit, attribute piece and shape piece bag slots.</param>
    private void Sew(GameSession session, Character c, byte[] rest)
    {
        if (rest.Length < 3 || session.Battle is not null)
        {
            session.Send(PoseMessages.Failed());
            return;
        }
        var (kitSlot, attrSlot, shapeSlot) = (rest[0], rest[1], rest[2]);
        var kit = c.Bag.Find(b => b.Slot == kitSlot);
        var attr = c.Bag.Find(b => b.Slot == attrSlot);
        var shape = c.Bag.Find(b => b.Slot == shapeSlot);
        var (ra, rs) = (attr is null ? null : items[attr.ItemId], shape is null ? null : items[shape.ItemId]);
        if (kit is null || kit.ItemId is not (SewingTools or WeaponModel) || attr is null || shape is null || ra is null || rs is null || attr == shape
            || !Fits(kit.ItemId, ra) || !Fits(kit.ItemId, rs) || ra.EquipSlot != rs.EquipSlot || ra.Type != rs.Type)
        {
            logger.LogInformation("[{Session}] {Name}: sewing refused (kit {Kit}, pieces {A} and {B})", session.Id, c.Name, kitSlot, attrSlot, shapeSlot);
            session.Send(PoseMessages.Failed());
            return;
        }
        attr.Pose = shape.Pose != 0 ? shape.Pose : shape.ItemId;
        Use(session, c, shape);
        Use(session, c, kit);
        session.Send(PoseMessages.Designed(attr.Slot, attr.Pose));
        logger.LogInformation("[{Session}] {Name}: {Attr} now looks like {Shape}", session.Id, c.Name, ra.Name.Trim(), rs.Name.Trim());
    }

    /// <summary>Takes the pose off a piece, spends the rebirth box and tells the client.</summary>
    /// <param name="session">The player's session.</param>
    /// <param name="c">The character who reverts.</param>
    /// <param name="rest">The bytes after the operation: box and piece bag slots.</param>
    private void Revert(GameSession session, Character c, byte[] rest)
    {
        if (rest.Length < 2 || session.Battle is not null)
        {
            session.Send(PoseMessages.RevertFailed());
            return;
        }
        var box = c.Bag.Find(b => b.Slot == rest[0]);
        var piece = c.Bag.Find(b => b.Slot == rest[1]);
        if (box is null || box.ItemId != RebirthBox || piece is null || piece.Pose == 0)
        {
            session.Send(PoseMessages.RevertFailed());
            return;
        }
        piece.Pose = 0;
        Use(session, c, box);
        session.Send(PoseMessages.Reverted());
        logger.LogInformation("[{Session}] {Name}: {Item} is back to its own look", session.Id, c.Name, items[piece.ItemId]?.Name.Trim());
    }

    /// <summary>Tells whether a piece may go into the form of a sewing kit.</summary>
    /// <param name="kit">The item id of the kit.</param>
    /// <param name="r">The item record of the piece.</param>
    /// <returns>True when the piece suits the kit's equipment slots, has a shop rank above 1 and is not team NPC gear.</returns>
    private static bool Fits(ushort kit, ItemRecord r) =>
        (kit == SewingTools ? r.EquipSlot is 1 or 2 or 4 or 5 : r.EquipSlot == 3 && r.Type != 11) // 3 WeaponSlot, 11 GunType
            && r.ShopColumn > 1 && (r.Words415.Length < 3 || r.Words415[2] == 0) && r.SubType != 149; // 149 BlessGearSubType

    /// <summary>Spends one item from a bag stack and tells the client.</summary>
    /// <param name="session">The player's session.</param>
    /// <param name="c">The character who owns the stack.</param>
    /// <param name="stack">The bag stack.</param>
    private static void Use(GameSession session, Character c, BagItem stack)
    {
        stack.Count--;
        session.Send(ItemMessages.BagSlotDecrease(stack.Slot, 1));
        if (stack.Count == 0)
            c.Bag.Remove(stack);
    }
}
