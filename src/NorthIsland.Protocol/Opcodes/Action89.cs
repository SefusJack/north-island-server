namespace NorthIsland.Protocol.Opcodes;

/// <summary>Sub codes of action 89, the client's requests for the activity list (the answer is sent as action 90).</summary>
public static class Action89
{
    /// <summary>The action number, 89.</summary>
    public const byte Code = 89;

    /// <summary>Client to server: the client asks for the activity list and sends the version of its cached copy.</summary>
    public const byte ActivityList = 0;

    /// <summary>Client to server: the client asks to go to the event zone of the activity with the given id.</summary>
    public const byte Teleport = 1;
}
