namespace NorthIsland.Protocol.Opcodes;

/// <summary>Action id 65: tents (camps): entering, listing, locking, styles and pillars.</summary>
public static class Action65
{
    /// <summary>Action id of this packet group, 65.</summary>
    public const byte Code = 65;

    /// <summary>Client to server: enters a tent (same value as <c>Added</c>).</summary>
    public const byte Enter = 1;

    /// <summary>Server to client: a tent was added to the scene (same value as <c>Enter</c>).</summary>
    public const byte Added = 1;

    /// <summary>Client to server: takes the tent down (same value as <c>SceneLimit</c>).</summary>
    public const byte TakeDown = 2;

    /// <summary>Server to client: the map already has the most tents allowed (same value as <c>TakeDown</c>).</summary>
    public const byte SceneLimit = 2;

    /// <summary>Server to client: the tents of the scene (same value as <c>Open</c>).</summary>
    public const byte List = 3;

    /// <summary>Client to server: opens the house to visitors (same value as <c>List</c>).</summary>
    public const byte Open = 3;

    /// <summary>Client to server: closes the house to visitors (same value as <c>TakenBack</c>).</summary>
    public const byte Close = 4;

    /// <summary>Client to server: changes the tent style.</summary>
    public const byte ChangeStyle = 12;

    /// <summary>Server to client: the style of an owner's tent changed.</summary>
    public const byte StyleChanged = 14;

    /// <summary>Server to client: an owner's tent was taken back (same value as <c>Close</c>).</summary>
    public const byte TakenBack = 4;

    /// <summary>Server to client: the server already has the most tents allowed.</summary>
    public const byte ServerLimit = 6;

    /// <summary>Server to client: the host left the house.</summary>
    public const byte HostLeft = 5;

    /// <summary>Server to client: the house is locked or unlocked, with a flag to show a message.</summary>
    public const byte Lock = 7;

    /// <summary>Server to client: entering the house is forbidden.</summary>
    public const byte Forbidden = 8;

    /// <summary>Server to client: the room size and floor (same value as <c>ChangeStyle</c>).</summary>
    public const byte Room = 12;

    /// <summary>Pillar changes, both directions.</summary>
    public const byte Pillars = 11;

    /// <summary>Server to client: the pillar differences of a floor.</summary>
    public const byte PillarDiff = 13;
}
