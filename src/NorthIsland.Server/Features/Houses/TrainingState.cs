namespace NorthIsland.Server.Features.Houses;

/// <summary>The running training of a player at a piece of furniture: their own, or one of their pet's.</summary>
/// <param name="furn">The index of the furniture piece the training happens at.</param>
/// <param name="petSlot">The pet slot of the training pet; 0 when the player trains themselves.</param>
/// <param name="next">The time of the next training step.</param>
public sealed class TrainingState(
    ushort furn,
    byte petSlot,
    DateTime next)
{
    /// <summary>The index of the furniture piece the training happens at.</summary>
    public ushort Furn { get; } = furn;

    /// <summary>The pet slot of the training pet; 0 when the player trains themselves.</summary>
    public byte PetSlot { get; } = petSlot;

    /// <summary>The time of the next training step; moved on after each step.</summary>
    public DateTime Next { get; set; } = next;
}
