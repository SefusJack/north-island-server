namespace NorthIsland.Server.Features.Social;

/// <summary>Settings of the puppet training.</summary>
public sealed class PuppetOptions
{
    /// <summary>Days a relation lasts.</summary>
    public int Days { get; set; } = 10;

    /// <summary>Percent of the helper's earned exp that is copied onto the card.</summary>
    public int SharePercent { get; set; } = 100;

    /// <summary>Lowest level a requester and a helper need.</summary>
    public int MinLevel { get; set; } = 10;
}
