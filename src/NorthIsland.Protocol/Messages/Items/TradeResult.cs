namespace NorthIsland.Protocol.Messages.Items;

/// <summary>Result code of a trade request or of a running trade, sent in <c>25/2</c>.</summary>
public enum TradeResult : byte
{
    /// <summary>Result 0: busy.</summary>
    Busy = 0,

    /// <summary>Result 1: the other player is not online.</summary>
    TargetOffline = 1,

    /// <summary>Result 2: the other player is busy.</summary>
    TargetBusy = 2,

    /// <summary>Result 3: the trade was cancelled.</summary>
    Cancelled = 3,

    /// <summary>Result 4: the trade completed.</summary>
    Success = 4,

    /// <summary>Result 5: the players are not in the same scene.</summary>
    DifferentScene = 5,

    /// <summary>Result 6: a side does not have the gold it offered.</summary>
    NoFunds = 6,

    /// <summary>Result 7: an offer has more items than fit in the other bag.</summary>
    TooManyItems = 7,

    /// <summary>Result 8: a player is in a battle.</summary>
    InBattle = 8,

    /// <summary>Result 11: the player is already trading.</summary>
    AlreadyTrading = 11,
}
