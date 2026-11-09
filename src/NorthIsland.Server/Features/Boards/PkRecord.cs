namespace NorthIsland.Server.Features.Boards;

/// <summary>One player's entry in the PK ranking.</summary>
public sealed class PkRecord
{
    /// <summary>The player's account id.</summary>
    public int PlayerId { get; set; }

    /// <summary>The player's last known character name.</summary>
    public string Name { get; set; } = "";

    /// <summary>The number of PK wins.</summary>
    public int Wins { get; set; }
}
