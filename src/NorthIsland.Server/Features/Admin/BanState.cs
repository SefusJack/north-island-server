namespace NorthIsland.Server.Features.Admin;

/// <summary>The saved form of the ban list.</summary>
public sealed class BanState
{
    /// <summary>The banned accounts.</summary>
    public List<Ban> Accounts { get; set; } = [];

    /// <summary>The banned addresses.</summary>
    public List<Ban> Addresses { get; set; } = [];
}
