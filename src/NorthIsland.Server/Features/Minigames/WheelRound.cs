namespace NorthIsland.Server.Features.Minigames;

/// <summary>The Lucky Wheel state of a player.</summary>
public sealed class WheelRound
{
    /// <summary>The pay kind the player chose: 1 for points, 2 for tokens.</summary>
    public byte PayKind { get; set; } = 1;

    /// <summary>The prize codes of the current wheel's slots.</summary>
    public byte[] Codes { get; set; } = [];

    /// <summary>Whether the next spin is doubled, costing and paying twice.</summary>
    public bool Doubled { get; set; }
}
