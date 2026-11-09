namespace NorthIsland.Server.Features.Vehicles;

/// <summary>Numbers for vehicle fuel, wear and fees.</summary>
public sealed class VehicleOptions
{
    /// <summary>The fuel one fuel item adds.</summary>
    public int FuelPerItem { get; set; } = 100;

    /// <summary>The most fuel a vehicle holds.</summary>
    public int FuelMax { get; set; } = 1000;

    /// <summary>The fuel a riding vehicle uses per minute; 0 means none.</summary>
    public double FuelPerMinute { get; set; }

    /// <summary>The mall points a parking deposit costs; 0 means free.</summary>
    public int ParkPoints { get; set; } = 1;

    /// <summary>The mall points a repair costs per rank of the vehicle.</summary>
    public double RepairPointsPerRank { get; set; } = 1;

    /// <summary>The wear a riding vehicle gains per minute; 0 means none.</summary>
    public double WearPerMinute { get; set; }

    /// <summary>The fuel a transmit trip takes from the ridden vehicle; 0 means a trip costs nothing.</summary>
    public int TransmitFuel { get; set; }
}
