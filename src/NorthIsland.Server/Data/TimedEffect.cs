namespace NorthIsland.Server.Data;

/// <summary>A temporary effect on a character (an experience bonus, a stat buff, ...) that ends at a fixed time.</summary>
public sealed class TimedEffect
{
    /// <summary>The kind of effect, a string key such as <c>stat3</c>.</summary>
    public string Kind { get; set; } = "";

    /// <summary>The strength of the effect, for example a multiplier.</summary>
    public double Value { get; set; }

    /// <summary>The level of the effect; applying one with a different value or level replaces the running effect.</summary>
    public int Level { get; set; }

    /// <summary>The id of the item that started the effect.</summary>
    public ushort Item { get; set; }

    /// <summary>The time the effect ends.</summary>
    public DateTime Until { get; set; }
}
