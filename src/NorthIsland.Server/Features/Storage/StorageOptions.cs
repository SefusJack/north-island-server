namespace NorthIsland.Server.Features.Storage;

/// <summary>The options of the storage feature.</summary>
public sealed class StorageOptions
{
    /// <summary>The Space Contract rules.</summary>
    public SpaceContractOptions SpaceContract { get; set; } = new();
}
