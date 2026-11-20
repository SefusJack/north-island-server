namespace NorthIsland.Server.Features.Stalls;

/// <summary>Checks that a bag stack offered in a stall is still the one the stall opened with.</summary>
public static class StallStock
{
    /// <summary>
    /// Gives the stack in a priced bag slot when it is still the stack the stall opened with: the same item with the same attributes, no more of
    /// them than are left on offer, and not the vehicle the seller rides. A stack swapped into the slot is not offered.
    /// </summary>
    /// <param name="seller">The seller's character.</param>
    /// <param name="owner">The seller's session, for the ridden vehicle.</param>
    /// <param name="slot">The bag slot.</param>
    /// <param name="offered">The copy of the stack taken when the stall opened, its count lowered by each sale.</param>
    /// <returns>The stack, or null when the slot no longer holds it.</returns>
    public static BagItem? Find(Character seller, GameSession owner, byte slot, BagItem offered) =>
        seller.Bag.Find(b => b.Slot == slot) is { } stack && GridStore.Same(stack, offered) && stack.Count > 0 && stack.Count <= offered.Count
            && owner.Vehicle?.BagSlot != slot
            ? stack
            : null;

    /// <summary>Takes a copy of a stack to remember what a stall offers in a slot.</summary>
    /// <param name="stack">The bag stack.</param>
    /// <returns>The copy, not in any grid.</returns>
    public static BagItem Snapshot(BagItem stack) => GridStore.Copy(stack, stack.Slot, stack.Count);
}
