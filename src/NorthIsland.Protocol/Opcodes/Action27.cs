namespace NorthIsland.Protocol.Opcodes;

/// <summary>Sub codes of action <c>27</c>: the NPC shop.</summary>
public static class Action27
{
    /// <summary>The action byte <c>27</c> that starts every packet of this group.</summary>
    public const byte Code = 27;

    /// <summary>Both ways: buys from the open shop (client request); the server answers with the buy result.</summary>
    public const byte Buy = 1;

    /// <summary>Both ways: sells to the open shop (client request); the server answers with the sell result.</summary>
    public const byte Sell = 2;

    /// <summary>Server to client: opens the form for selling goods; no data.</summary>
    public const byte OpenSellGoods = 3;

    /// <summary>Server to client: opens the form for selling equipment; no data.</summary>
    public const byte OpenSellEquipment = 4;

    /// <summary>Server to client: the player's Lucky 7 count.</summary>
    public const byte Lucky7Count = 5;
}
