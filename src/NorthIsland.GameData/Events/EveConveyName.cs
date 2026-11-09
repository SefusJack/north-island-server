namespace NorthIsland.GameData.Events;

/// <summary>One entry of the convey name table after the scenes of the archive.</summary>
/// <param name="MapId">The map the convey belongs to.</param>
/// <param name="ConveyIndex">The convey index within that map.</param>
/// <param name="Name">The name, empty in the shipped data.</param>
public readonly record struct EveConveyName(
    ushort MapId,
    byte ConveyIndex,
    string Name)
{
    /// <summary>Size in bytes of one entry in the file.</summary>
    public const int Size = 24;
}
