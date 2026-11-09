namespace NorthIsland.Server.Features.Inn;

/// <summary>The inn state of one session.</summary>
public sealed class InnSessionState
{
    /// <summary>The price of the rest that was offered and not yet answered; null when no rest is on offer.</summary>
    public int? PendingPrice { get; set; }
}
