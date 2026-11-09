namespace NorthIsland.Server.Features.Battles;

/// <summary>Which stage of a round a battle is in.</summary>
public enum BattlePhase
{
    /// <summary>The players are choosing their commands.</summary>
    Command,

    /// <summary>The server is playing the round's actions.</summary>
    Acting,

    /// <summary>The battle has ended.</summary>
    Over,
}
