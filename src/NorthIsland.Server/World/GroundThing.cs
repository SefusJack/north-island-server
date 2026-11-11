namespace NorthIsland.Server.World;

/// <summary>Something on the ground or picked up from it: an item stack or an amount of gold.</summary>
/// <param name="ItemId">The item id.</param>
/// <param name="Count">How many items.</param>
/// <param name="Gold">The gold amount, 0 for an item.</param>
/// <param name="Wear">The wear value of the item.</param>
/// <param name="Owner">The login id of the only player who may pick it up (a dropped item that cannot be transferred), 0 for anyone.</param>
public sealed record GroundThing(
    ushort ItemId,
    int Count,
    int Gold,
    byte Wear = 1,
    int Owner = 0);
