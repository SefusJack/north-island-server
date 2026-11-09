namespace NorthIsland.Server.Features.Mall;

/// <summary>Settings of the mall boxes.</summary>
public sealed class MallBoxOptions
{
    /// <summary>Path of an optional JSON file whose lucky bag tables replace the built-in ones; empty for none.</summary>
    public string LuckyBagsFile { get; set; } = "";
}
