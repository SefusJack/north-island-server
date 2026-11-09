namespace NorthIsland.Server.Features.Guilds;

/// <summary>The settings of the guilds feature.</summary>
public sealed class GuildFeatureOptions
{
    /// <summary>The Trojan War schedule.</summary>
    public TrojanWarOptions TrojanWar { get; set; } = new();

    /// <summary>The rules for guilds, such as the dismissal checks.</summary>
    public GuildOptions Guilds { get; set; } = new();
}
