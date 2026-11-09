namespace NorthIsland.Server.Features.Marriage;

/// <summary>A paid marriage application that waits for the wedding.</summary>
public sealed class MarriageApplication
{
    /// <summary>Login id of the groom.</summary>
    public int GroomId { get; set; }

    /// <summary>Login id of the bride.</summary>
    public int BrideId { get; set; }

    /// <summary>When the application expires (UTC).</summary>
    public DateTime UntilUtc { get; set; }

    /// <summary>Tells whether a player is one of the two applicants.</summary>
    /// <param name="id">The player's login id.</param>
    /// <returns>True when the id is the groom's or the bride's.</returns>
    public bool Has(int id) => GroomId == id || BrideId == id;
}
