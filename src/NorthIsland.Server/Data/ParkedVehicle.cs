namespace NorthIsland.Server.Data;

/// <summary>A vehicle left in the character's parking lot.</summary>
public sealed class ParkedVehicle
{
    /// <summary>Parking cell it stands in.</summary>
    public byte Cell { get; set; }

    /// <summary>Item id of the vehicle.</summary>
    public ushort ItemId { get; set; }

    /// <summary>The item's used amount, kept as it was in the bag.</summary>
    public int Used { get; set; }

    /// <summary>The item's extra amount, kept as it was in the bag.</summary>
    public ushort Extra { get; set; }

    /// <summary>The item's wear value, kept as it was in the bag.</summary>
    public byte Wear { get; set; } = 1;
}
