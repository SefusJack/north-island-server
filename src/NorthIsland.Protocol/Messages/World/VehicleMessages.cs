using NorthIsland.Protocol.IO;
using NorthIsland.Protocol.Opcodes;

namespace NorthIsland.Protocol.Messages.World;

/// <summary>Builders for the vehicle packets (action 15).</summary>
public static class VehicleMessages
{
    /// <summary>Builds the packet that shows a vehicle placed in the scene.</summary>
    /// <param name="bagSlot">Bag slot of the vehicle item.</param>
    /// <param name="ownerId">Id of the owner.</param>
    /// <param name="itemId">Item id of the vehicle.</param>
    /// <param name="x">X position in pixels.</param>
    /// <param name="y">Y position in pixels.</param>
    /// <param name="board">True to send the variant after which the owner boards.</param>
    /// <returns>The packet.</returns>
    public static byte[] Placed(
        byte bagSlot,
        int ownerId,
        ushort itemId,
        int x,
        int y,
        bool board = false) =>
            new PacketWriter(Action15.Code, board ? Action15.PlacedAndBoard : Action15.Placed)
                .U8(bagSlot).I32(ownerId).U16(itemId).I32(x).I32(y).ToArray();

    /// <summary>Builds the packet that shows a player boarding a vehicle.</summary>
    /// <param name="bagSlot">Bag slot of the vehicle item.</param>
    /// <param name="humanId">Id of the player.</param>
    /// <param name="itemId">Item id of the vehicle.</param>
    /// <returns>The packet.</returns>
    public static byte[] Boarded(byte bagSlot, int humanId, ushort itemId) =>
        new PacketWriter(Action15.Code, Action15.Boarded).U8(bagSlot).I32(humanId).U16(itemId).ToArray();

    /// <summary>Builds the packet that puts the placed vehicle back into the bag.</summary>
    /// <param name="bagSlot">Bag slot of the vehicle item.</param>
    /// <returns>The packet.</returns>
    public static byte[] TakenBack(byte bagSlot) => new PacketWriter(Action15.Code, Action15.TakenBack).U8(bagSlot).ToArray();

    /// <summary>Builds the packet that shows a vehicle being destroyed.</summary>
    /// <param name="ownerId">Id of the owner.</param>
    /// <param name="itemId">Item id of the vehicle.</param>
    /// <returns>The packet.</returns>
    public static byte[] Destroyed(int ownerId, ushort itemId) =>
        new PacketWriter(Action15.Code, Action15.Destroyed).I32(ownerId).U16(itemId).ToArray();

    /// <summary>Builds the packet with the wear and fuel of the owner's vehicle.</summary>
    /// <param name="bagSlot">Bag slot of the vehicle item.</param>
    /// <param name="ownerId">Id of the owner.</param>
    /// <param name="wear">Wear value.</param>
    /// <param name="fuel">Fuel amount.</param>
    /// <returns>The packet.</returns>
    public static byte[] Wear(byte bagSlot, int ownerId, int wear, ushort fuel) =>
        new PacketWriter(Action15.Code, Action15.Wear).U8(bagSlot).I32(ownerId).I32(wear).U16(fuel).ToArray();

    /// <summary>Builds the packet that tells the player the vehicle has no fuel.</summary>
    /// <returns>The packet.</returns>
    public static byte[] NoFuel() => new PacketWriter(Action15.Code, Action15.NoFuel).ToArray();

    /// <summary>Builds the packet that removes a vehicle from the scene.</summary>
    /// <param name="ownerId">Id of the owner.</param>
    /// <param name="itemId">Item id of the vehicle.</param>
    /// <returns>The packet.</returns>
    public static byte[] Removed(int ownerId, ushort itemId) =>
        new PacketWriter(Action15.Code, Action15.Removed).I32(ownerId).U16(itemId).ToArray();

    /// <summary>Builds the packet that shows a player leaving a vehicle.</summary>
    /// <param name="bagSlot">Bag slot of the vehicle item.</param>
    /// <param name="humanId">Id of the player.</param>
    /// <returns>The packet.</returns>
    public static byte[] Dismounted(byte bagSlot, int humanId) =>
        new PacketWriter(Action15.Code, Action15.Dismounted).U8(bagSlot).I32(humanId).ToArray();
}
