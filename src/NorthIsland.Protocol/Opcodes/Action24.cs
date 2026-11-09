namespace NorthIsland.Protocol.Opcodes;

/// <summary>Sub codes of action <c>24</c>: quest marks held by the player and the mark flags.</summary>
public static class Action24
{
    /// <summary>The action byte <c>24</c> that starts every packet of this group.</summary>
    public const byte Code = 24;

    /// <summary>Server to client: adds a quest mark with its count.</summary>
    public const byte Add = 1;

    /// <summary>Server to client: removes a quest mark.</summary>
    public const byte Delete = 4;

    /// <summary>Client to server: the quest window's Delete button for a mark.</summary>
    public const byte Abandon = 2;

    /// <summary>Client to server: sent on a map change while the player holds certain mark flags; the server has nothing to answer.</summary>
    public const byte MapChangeMarks = 5;

    /// <summary>Server to client: sets or clears one mark flag bit.</summary>
    public const byte SetFlag = 5;

    /// <summary>Server to client: the mark flag bytes as pairs of byte index and bits.</summary>
    public const byte Flags = 7;

    /// <summary>Server to client: the quest marks as records of slot, mark id and count.</summary>
    public const byte List = 6;
}
