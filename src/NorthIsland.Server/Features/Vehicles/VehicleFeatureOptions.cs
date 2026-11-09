namespace NorthIsland.Server.Features.Vehicles;

/// <summary>The configuration section of the vehicles feature.</summary>
public sealed class VehicleFeatureOptions
{
    /// <summary>The vehicle settings.</summary>
    public VehicleOptions Vehicles { get; set; } = new();
}
