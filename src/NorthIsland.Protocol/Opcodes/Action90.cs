namespace NorthIsland.Protocol.Opcodes;

/// <summary>Sub codes of action 90, the activity list and its notices (server to client) and the gift card lists (client to server).</summary>
public static class Action90
{
    /// <summary>The action number, 90.</summary>
    public const byte Code = 90;

    /// <summary>Server to client: the activity list for a weekday, with the list version.</summary>
    public const byte List = 0;

    /// <summary>
    /// Sub code of the activity states sent to the client; the server also routes it when the client sends it to request the gift card lists.
    /// </summary>
    public const byte State = 1;

    /// <summary>Server to client: a notice of one kind byte for the activity list.</summary>
    public const byte Notice = 2;
}
