namespace NorthIsland.Server.Features.Battles;

/// <summary>Which fighters of a side count when the average level for the combination odds is taken.</summary>
public enum ComboOddsLevels
{
    /// <summary>Every fighter still standing on the side: players, pets and monsters.</summary>
    All,

    /// <summary>Only the standing players of the side (everyone when it has none).</summary>
    Players,

    /// <summary>Only the fighters that already take part in the combination plus the one about to join.</summary>
    Members,
}
