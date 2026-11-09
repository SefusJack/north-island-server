namespace NorthIsland.Server.Features.Battles;

/// <summary>How the Hot Fire Attack buff is applied to the damage formulas.</summary>
public enum HotFireMode
{
    /// <summary>The buff scales the attack and magic attack before the damage factor.</summary>
    Attack,

    /// <summary>The buff scales the finished hit.</summary>
    Damage,
}
