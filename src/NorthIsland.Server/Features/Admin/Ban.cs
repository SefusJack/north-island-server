using System.Net;

namespace NorthIsland.Server.Features.Admin;

/// <summary>One ban entry, for an account name or an address.</summary>
public sealed class Ban
{
    /// <summary>The banned account name or IP address text.</summary>
    public string Target { get; set; } = "";

    /// <summary>The account the ban was issued for.</summary>
    public string Account { get; set; } = "";

    /// <summary>The name of the GM character who issued the ban.</summary>
    public string By { get; set; } = "";

    /// <summary>When the ban was issued, in UTC.</summary>
    public DateTime Utc { get; set; }
}
