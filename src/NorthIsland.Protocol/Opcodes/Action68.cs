namespace NorthIsland.Protocol.Opcodes;

/// <summary>Action id 68: the item bag (mini grid).</summary>
public static class Action68
{
    /// <summary>Action id of this packet group, 68.</summary>
    public const byte Code = 68;

    /// <summary>Client to server: moves items from the item bag to the bag.</summary>
    public const byte ToBag = 1;

    /// <summary>Client to server: moves items inside the item bag.</summary>
    public const byte Move = 2;

    /// <summary>Client to server: moves bag items into the item bag.</summary>
    public const byte ToGrid = 3;

    /// <summary>Server to client: the item bag contents.</summary>
    public const byte List = 4;

    /// <summary>Server to client: opens the item bag form.</summary>
    public const byte Open = 9;
}
