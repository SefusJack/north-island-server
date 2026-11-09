using NorthIsland.GameData.Items;
using NorthIsland.Protocol.Messages.Items;

namespace NorthIsland.Server.Features.Items;

/// <summary>Items that can be used a limited number of times, counted in the wear of their stack.</summary>
public static class UseLimit
{
    /// <summary>Spends one use of an item: adds a share to the stack's wear and removes the stack when the wear reaches 250.</summary>
    /// <param name="session">The player's session.</param>
    /// <param name="character">The player's character.</param>
    /// <param name="stack">The bag stack.</param>
    /// <param name="item">The definition of the item, which holds the number of uses.</param>
    public static void Spend(GameSession session, Character character, BagItem stack, ItemRecord item)
    {
        var maxUses = Math.Max((ushort)1, item.Words429.FirstOrDefault());
        var wear = stack.Wear + 250 / maxUses;
        if (wear >= 250)
        {
            character.Bag.Remove(stack);
            session.Send(ItemMessages.BagSlotDecrease(stack.Slot, stack.Count));
            return;
        }
        stack.Wear = (byte)wear;
        session.Send(ItemMessages.BagWear(stack.Slot, wear));
    }
}
