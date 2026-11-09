namespace NorthIsland.Protocol.Opcodes;

/// <summary>Action id 53: battle effects (knock-off, thrown items, fighter status) and the monster book.</summary>
public static class Action53
{
    /// <summary>Action id of this packet group, 53.</summary>
    public const byte Code = 53;

    /// <summary>Server to client: the fighter at a row and column is knocked off.</summary>
    public const byte KnockedOff = 3;

    /// <summary>Server to client: an item icon flies from a fighter to a list of cells.</summary>
    public const byte ThrowItem = 4;

    /// <summary>Server to client: a monster was added to the monster book.</summary>
    public const byte BookAdded = 9;

    /// <summary>Server to client: the monster book bits, as group and bit-set pairs.</summary>
    public const byte BookBits = 10;

    /// <summary>Server to client: a status effect on the fighter at a row and column.</summary>
    public const byte Status = 1;
}
