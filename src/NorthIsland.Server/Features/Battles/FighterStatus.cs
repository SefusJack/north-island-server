namespace NorthIsland.Server.Features.Battles;

/// <summary>A status effect on a fighter.</summary>
public sealed class FighterStatus
{
    /// <summary>The status slot attribute (the 50/1 value attr) that carries the effect.</summary>
    public required byte Attr { get; init; }

    /// <summary>The skill that caused the status; its icon is shown.</summary>
    public required ushort SkillId { get; init; }

    /// <summary>The Skill.dat effect byte.</summary>
    public required byte Effect { get; init; }

    /// <summary>The last battle round in which the status is active.</summary>
    public required int LastRound { get; set; }

    /// <summary>The amount the status applies each round, for example the damage of a poison; 0 when it has none.</summary>
    public int Amount { get; init; }
}
