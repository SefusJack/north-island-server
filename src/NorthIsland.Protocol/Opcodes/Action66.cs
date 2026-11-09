namespace NorthIsland.Protocol.Opcodes;

/// <summary>Action id 66: the reborn wish.</summary>
public static class Action66
{
    /// <summary>Action id of this packet group, 66.</summary>
    public const byte Code = 66;

    /// <summary>Client to server: the reborn wish (same value as <c>WishList</c>).</summary>
    public const byte Wish = 11;

    /// <summary>Server to client: the wish list (same value as <c>Wish</c>).</summary>
    public const byte WishList = 11;

    /// <summary>Sub code 12; no code uses it.</summary>
    public const byte Show = 12;

    /// <summary>Server to client: removes an NPC from the wish.</summary>
    public const byte Removed = 13;

    /// <summary>Sub code 14; no code uses it.</summary>
    public const byte WishIds = 14;

    /// <summary>Server to client: the choice step of the reborn wish.</summary>
    public const byte Choice = 15;

    /// <summary>Server to client: the greeting of the reborn wish.</summary>
    public const byte Greeting = 16;
}
