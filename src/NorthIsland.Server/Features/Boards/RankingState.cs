namespace NorthIsland.Server.Features.Boards;

/// <summary>The saved state of the rankings.</summary>
public sealed class RankingState
{
    /// <summary>The PK wins of every player who has won a PK battle.</summary>
    public List<PkRecord> Pk { get; set; } = [];

    /// <summary>The names in the quiz ranking, best first.</summary>
    public List<string> Quiz { get; set; } = [];

    /// <summary>The names in the event ranking, best first.</summary>
    public List<string> Compass { get; set; } = [];
}
