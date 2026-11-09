namespace NorthIsland.Protocol.Opcodes;

/// <summary>Sub codes of action 91, the Lucky Bag preview.</summary>
public static class Action91
{
    /// <summary>The action number, 91.</summary>
    public const byte Code = 91;

    /// <summary>Client to server: the client asks what a Lucky Bag can contain.</summary>
    public const byte Request = 1;

    /// <summary>Server to client: the contents of a Lucky Bag, starting with the bag's item id.</summary>
    public const byte Info = 2;
}
