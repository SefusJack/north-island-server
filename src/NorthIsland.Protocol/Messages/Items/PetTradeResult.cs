namespace NorthIsland.Protocol.Messages.Items;

/// <summary>Result of a pet trade request, sent to the client as one byte (25/11).</summary>
public enum PetTradeResult : byte
{
    /// <summary>The player is already in a trade.</summary>
    Busy = 0,

    /// <summary>The other player is not online.</summary>
    TargetOffline = 1,

    /// <summary>The other player is busy with a trade or an event.</summary>
    TargetBusy = 2,

    /// <summary>The trade was cancelled.</summary>
    Cancelled = 3,

    /// <summary>The trade went through.</summary>
    Success = 4,

    /// <summary>The other player is on another map.</summary>
    DifferentScene = 5,

    /// <summary>A player does not have the offered gold.</summary>
    NoFunds = 6,

    /// <summary>One of the players is in a battle.</summary>
    InBattle = 8,

    /// <summary>The swap would give a player more pets than allowed.</summary>
    TooManyPets = 10,

    /// <summary>A player would receive a pet of a kind they already have.</summary>
    SamePet = 11,

    /// <summary>The offered pet has no amity.</summary>
    NoAmity = 12,
}
