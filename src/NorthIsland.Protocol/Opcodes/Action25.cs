namespace NorthIsland.Protocol.Opcodes;

/// <summary>Sub codes of action <c>25</c>: item trade between players, the large-item and pet trades, and the pet express delivery.</summary>
public static class Action25
{
    /// <summary>The action byte <c>25</c> that starts every packet of this group.</summary>
    public const byte Code = 25;

    /// <summary>Client to server: asks another player to trade.</summary>
    public const byte Request = 1;

    /// <summary>Server to client: a large-item trade was refused because a side is under level 10; no payload.</summary>
    public const byte LevelNotice = 43;

    /// <summary>Both ways: opens the trade of large items with a target player.</summary>
    public const byte LargeRequest = 40;

    /// <summary>Both ways: one side's offer of gold and large items in a large-item trade.</summary>
    public const byte LargeOffer = 41;

    /// <summary>Both ways: confirms or cancels a large-item trade, and carries its result code to the client.</summary>
    public const byte LargeDecide = 42;

    /// <summary>Both ways: opens the pet trade with a target player.</summary>
    public const byte PetRequest = 10;

    /// <summary>Both ways: one side's offer of a pet and gold in a pet trade; the server answers with a result.</summary>
    public const byte PetOffer = 11;

    /// <summary>Both ways: confirms or cancels a pet trade; the server sends the partner's offer.</summary>
    public const byte PetDecide = 12;

    /// <summary>Both ways: a pet carries bag items to another player; the server reports started or over.</summary>
    public const byte Express = 20;

    /// <summary>Both ways: the pet walks off or comes back on the owner's screen.</summary>
    public const byte ExpressStop = 21;

    /// <summary>Server to client: why an express delivery failed.</summary>
    public const byte ExpressError = 22;

    /// <summary>Server to client: the delivered item as seen by the receiver or the sender.</summary>
    public const byte ExpressResult = 23;

    /// <summary>Client to server: the player's offer of gold and items in an item trade.</summary>
    public const byte Offer = 2;

    /// <summary>Client to server: confirms or cancels an item trade.</summary>
    public const byte Decide = 3;

    /// <summary>Server to client: opens the trade window with a partner.</summary>
    public const byte Start = 1;

    /// <summary>Server to client: result code of an item trade; closes the window.</summary>
    public const byte Result = 2;

    /// <summary>Server to client: the partner's gold and items in an item trade.</summary>
    public const byte PartnerOffer = 3;

    /// <summary>Server to client: whether the player has been online long enough to trade.</summary>
    public const byte OnlineTime = 44;
}
