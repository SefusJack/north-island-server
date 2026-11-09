namespace NorthIsland.Server.Hosting;

/// <summary>The server's source of random numbers, with an override per async flow so tests can fix the outcome.</summary>
public static class GameRandom
{
    /// <summary>The replacement random source of the current async flow, if any.</summary>
    private static readonly AsyncLocal<Random?> s_override = new();

    /// <summary>The random source to use: the override when one is set, otherwise the shared one.</summary>
    public static Random Shared => s_override.Value ?? Random.Shared;

    /// <summary>Sets or clears the replacement random source of the current async flow.</summary>
    /// <param name="random">The source to use, or null to use the shared one.</param>
    public static void Use(Random? random) => s_override.Value = random;
}
