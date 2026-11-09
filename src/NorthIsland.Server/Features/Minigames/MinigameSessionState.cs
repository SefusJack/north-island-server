namespace NorthIsland.Server.Features.Minigames;

/// <summary>The minigame state kept on a player's session.</summary>
public sealed class MinigameSessionState
{
    /// <summary>The sport kind of the game started from the item mall that is running; null when none.</summary>
    public byte? MallMinigame { get; set; }

    /// <summary>The Lucky Cards play in progress; null when none.</summary>
    public PokeRound? Poke { get; set; }

    /// <summary>The Lucky Wheel's current wheel and settings; null until the wheel is first used.</summary>
    public WheelRound? Wheel { get; set; }
}
