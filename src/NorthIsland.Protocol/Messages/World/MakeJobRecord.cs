namespace NorthIsland.Protocol.Messages.World;

/// <summary>One running manufacturing job in a house.</summary>
/// <param name="Slot">Job slot.</param>
/// <param name="Product">Item id being made.</param>
/// <param name="ElapsedMs">Milliseconds the job has run.</param>
/// <param name="Batches">Number of batches.</param>
public readonly record struct MakeJobRecord(
    byte Slot,
    ushort Product,
    int ElapsedMs,
    byte Batches);
