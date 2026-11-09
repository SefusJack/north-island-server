namespace NorthIsland.Server.Features.PetCare;

/// <summary>A pet search in progress.</summary>
/// <param name="BoneSlot">The bag slot of the Dog Bone.</param>
/// <param name="PetSlot">The slot of the searching pet.</param>
/// <param name="ItemId">The item the pet looks for.</param>
/// <param name="Due">When the pet comes back.</param>
public sealed record ForageJob(
    byte BoneSlot,
    byte PetSlot,
    ushort ItemId,
    DateTime Due);
