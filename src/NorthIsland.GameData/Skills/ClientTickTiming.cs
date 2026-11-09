namespace NorthIsland.GameData.Skills;

/// <summary>The costs of one tick of the client's battle loop, used by <see cref="ClientTickModel"/>.</summary>
public sealed record ClientTickTiming
{
    /// <summary>The interval in ms at which the frame timer posts a tick.</summary>
    public double BeatMs { get; init; } = 30;

    /// <summary>The sleep in ms of a tick in which a fighter holds on a point.</summary>
    public double HoldSleepMs { get; init; } = 35;

    /// <summary>Extra ms a sleep takes beyond its nominal time.</summary>
    public double SleepOvershootMs { get; init; } = 0.5;

    /// <summary>The cost in ms of a tick besides its sleeps: drawing and blitting.</summary>
    public double DrawMs { get; init; } = 14;

    /// <summary>The interval in ms between the monitor's vertical blanks; 0 or less turns the wait off.</summary>
    public double VerticalBlankMs { get; init; } = 1000.0 / 60;

    /// <summary>Extra ms added to the attacker's time when the motion moves the camera, for the camera's return.</summary>
    public double CameraReturnMs { get; init; } = 250;

    /// <summary>The number of vertical blank phases the simulation is averaged over.</summary>
    public int Phases { get; init; } = 4;
}
