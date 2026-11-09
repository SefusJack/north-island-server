namespace NorthIsland.GameData.Skills;

/// <summary>One sound of a motion.</summary>
/// <param name="SoundId">The sound id.</param>
/// <param name="Delay">The delay of the sound.</param>
/// <param name="Flag">A byte stored with the sound; the code does not interpret it.</param>
public readonly record struct MotionSound(
    ushort SoundId,
    int Delay,
    byte Flag);
