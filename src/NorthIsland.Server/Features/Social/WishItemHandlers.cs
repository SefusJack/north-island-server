using System.Buffers.Binary;
using Microsoft.Extensions.Logging;
using NorthIsland.Protocol.Messages.Items;
using NorthIsland.Protocol.Text;

namespace NorthIsland.Server.Features.Social;

/// <summary>Handles the Bliss Lamp and the Passion Bouquet, items that send a message to another player.</summary>
/// <param name="world">World state.</param>
/// <param name="logger">Logger.</param>
public sealed class WishItemHandlers(
    WorldState world,
    ILogger<WishItemHandlers> logger)
{
    /// <summary>Item ids of the Bliss Lamp and of the Passion Bouquet.</summary>
    public const ushort BlissLamp = 34325, PassionBouquet = 34233; // 34325 Bliss Lamp, 34233 Passion Bouquet

    /// <summary>
    /// Handles sending a message with the item: checks the item, the target, the block list and battle, uses up one item and shows the message.
    /// </summary>
    /// <param name="session">The sender's session.</param>
    /// <param name="body">The packet body: bag slot, the target's login id, text length, text.</param>
    /// <param name="lamp">True for the Lamp, false for the Bouquet.</param>
    internal ValueTask OnSend(GameSession session, ReadOnlyMemory<byte> body, bool lamp)
    {
        if (session.State != SessionState.InWorld
            || session.ActiveCharacter is not { } c
            || session.Account is null
            || body.Length < 6
            || session.Battle is not null)
            return ValueTask.CompletedTask;
        var (slot, targetId) = (body.Span[0], BinaryPrimitives.ReadInt32LittleEndian(body.Span[1..]));
        var length = body.Span[5];
        var text = GameEncoding.GetString(body.Span[6..]);
        var stack = c.Bag.Find(b => b.Slot == slot);
        var myId = session.Account.LoginId;
        if (stack?.ItemId != (lamp ? BlissLamp : PassionBouquet) || length == 0 || text.Length == 0)
        {
            session.Send(Refuse(lamp, 0));
            return ValueTask.CompletedTask;
        }
        if (world.SessionOf(targetId) is not { } target || target.ActiveCharacter is not { } t)
        {
            session.Send(Refuse(lamp, lamp ? (byte)0 : (byte)3));
            return ValueTask.CompletedTask;
        }
        if (BlocklistHandlers.Blocks(t, myId))
        {
            session.Send(Refuse(lamp, lamp ? (byte)2 : (byte)1));
            return ValueTask.CompletedTask;
        }
        if (target.Battle is not null)
        {
            session.Send(Refuse(lamp, lamp ? (byte)3 : (byte)2));
            return ValueTask.CompletedTask;
        }
        stack.Count--;
        session.Send(ItemMessages.BagSlotDecrease(stack.Slot, 1));
        if (stack.Count == 0)
            c.Bag.Remove(stack);
        if (lamp)
        {
            var packet = WishMessages.BlessDone(myId, targetId, text);
            session.Send(packet);
            world.Broadcast(session, packet, sameMapOnly: true);
            if (t.MapId != c.MapId)
                target.Send(packet);
        }
        else
        {
            session.Send(WishMessages.FlowerToSender(myId, targetId, text));
            var packet = WishMessages.FlowerToTarget(myId, targetId, text);
            target.Send(packet);
            foreach (var other in world.OnMap(t.MapId))
                if (!ReferenceEquals(other, target) && !ReferenceEquals(other, session))
                    other.Send(packet);
        }
        logger.LogInformation(
            "[{Session}] {Name} sends {Item} to {Target}: {Text}",
            session.Id,
            c.Name,
            lamp ? "a Bliss Lamp" : "a Passion Bouquet",
            t.Name,
            text);
        return ValueTask.CompletedTask;
    }

    /// <summary>Builds the refusal packet for the item.</summary>
    /// <param name="lamp">True for the Lamp.</param>
    /// <param name="code">The refusal code.</param>
    /// <returns>The packet bytes.</returns>
    private static byte[] Refuse(bool lamp, byte code) => lamp ? WishMessages.BlessRefused(code) : WishMessages.FlowerRefused(code);
}
