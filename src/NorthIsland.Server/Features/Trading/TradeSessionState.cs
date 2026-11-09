namespace NorthIsland.Server.Features.Trading;

/// <summary>The trades a session takes part in.</summary>
public sealed class TradeSessionState
{
    /// <summary>The item trade of the session, or null.</summary>
    public Trade? Items { get; set; }

    /// <summary>The pet trade of the session, or null.</summary>
    public Trade? Pets { get; set; }
}
