namespace NorthIsland.Server.Features.Trading;

/// <summary>The options of the trade feature.</summary>
public sealed class TradeOptions
{
    /// <summary>The hours an account must have been online before it may trade.</summary>
    public double TradeOnlineHours { get; set; } = 6;
}
