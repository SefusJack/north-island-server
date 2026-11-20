namespace NorthIsland.Server.Features.Inn;

/// <summary>The inn state of one session.</summary>
public sealed class InnSessionState
{
    /// <summary>The price of the rest that was offered and not yet answered; null when no rest is on offer.</summary>
    public int? PendingPrice { get; set; }

    /// <summary>The event run that offered the rest; the offer only stands while this run is the session's running event.</summary>
    public EventRun? OfferRun { get; set; }

    /// <summary>The map id the rest was offered on.</summary>
    public ushort OfferMap { get; set; }
}
