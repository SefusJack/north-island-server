namespace NorthIsland.Server.Features.Battles;

/// <summary>What decides whether a further attacker joins a combination.</summary>
public enum ComboJoinRule
{
    /// <summary>Turn speed alone: a fighter within <see cref="BattleOptions.ComboSpeedRange"/> always joins.</summary>
    Speed,

    /// <summary>Turn speed plus a roll on the level lead of the attacking side over the target.</summary>
    Level,
}
