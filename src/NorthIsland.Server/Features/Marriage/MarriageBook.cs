namespace NorthIsland.Server.Features.Marriage;

/// <summary>The stored marriage data: couples, applications and divorce records.</summary>
public sealed class MarriageBook
{
    /// <summary>All married couples.</summary>
    public List<Couple> Couples { get; set; } = [];

    /// <summary>The applications that have not expired or been used.</summary>
    public List<MarriageApplication> Applications { get; set; } = [];

    /// <summary>When each player was last divorced (UTC), by login id; used for the wait before a new application.</summary>
    public Dictionary<int, DateTime> DivorcedUtc { get; set; } = [];

    /// <summary>Login ids of players who were divorced while offline and have not been told yet.</summary>
    public List<int> DivorcedWhileAway { get; set; } = [];
}
