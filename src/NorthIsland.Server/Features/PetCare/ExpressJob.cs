namespace NorthIsland.Server.Features.PetCare;

/// <summary>A pet express trip in progress.</summary>
/// <param name="PetId">The NPC id of the pet that carries.</param>
/// <param name="TargetId">The login id of the receiving player.</param>
/// <param name="Stacks">The bag slots and counts carried.</param>
/// <param name="Due">When the pet arrives.</param>
public sealed record ExpressJob(
    int PetId,
    int TargetId,
    List<(byte Slot, byte Count)> Stacks,
    DateTime Due);
