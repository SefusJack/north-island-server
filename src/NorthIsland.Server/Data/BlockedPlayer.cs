namespace NorthIsland.Server.Data;

/// <summary>A player on a character's blocklist.</summary>
public sealed class BlockedPlayer
{
    /// <summary>The blocked player's id.</summary>
    public int Id { get; set; }

    /// <summary>The blocked player's name.</summary>
    public string Name { get; set; } = "";
}
