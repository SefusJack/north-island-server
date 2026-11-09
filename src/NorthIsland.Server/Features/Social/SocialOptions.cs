namespace NorthIsland.Server.Features.Social;

/// <summary>Settings of the social feature.</summary>
public sealed class SocialOptions
{
    /// <summary>Settings of the puppet training.</summary>
    public PuppetOptions Puppet { get; set; } = new();
}
