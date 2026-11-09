namespace NorthIsland.Server.Features.Battles;

/// <summary>What one fighter did to one monster.</summary>
public sealed class MonsterContact
{
    /// <summary>The number of actions against the monster.</summary>
    public int Actions { get; set; }

    /// <summary>The total damage done to the monster.</summary>
    public long Damage { get; set; }
}
