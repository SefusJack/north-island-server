namespace NorthIsland.GameData.Skills;

/// <summary>One step of a motion's camera script.</summary>
/// <param name="Point">The point the camera moves to.</param>
/// <param name="Speed">The camera speed.</param>
/// <param name="Trigger">When the step starts.</param>
/// <param name="Focus">What the camera follows.</param>
public readonly record struct MotionCameraStep(
    MotionPoint Point,
    double Speed,
    int Trigger,
    byte Focus);
