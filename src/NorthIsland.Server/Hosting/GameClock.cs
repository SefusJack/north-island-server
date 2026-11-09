namespace NorthIsland.Server.Hosting;

/// <summary>The server's clock, with an override per async flow so tests can fix the time.</summary>
public static class GameClock
{
    /// <summary>The replacement clock of the current async flow, if any.</summary>
    private static readonly AsyncLocal<Func<DateTime>?> s_override = new();

    /// <summary>The current local time, from the override when one is set.</summary>
    public static DateTime Now => s_override.Value is { } clock ? DateTime.SpecifyKind(clock(), DateTimeKind.Local) : DateTime.Now;

    /// <summary>The current UTC time, from the override when one is set.</summary>
    public static DateTime UtcNow => s_override.Value is { } clock ? DateTime.SpecifyKind(clock(), DateTimeKind.Utc) : DateTime.UtcNow;

    /// <summary>Sets or clears the replacement clock of the current async flow.</summary>
    /// <param name="clock">A function that gives the time, or null to use the system clock.</param>
    public static void Use(Func<DateTime>? clock) => s_override.Value = clock;
}
