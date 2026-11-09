namespace NorthIsland.Protocol.Opcodes;

/// <summary>Sub codes of action <c>43</c>: alliances between guilds.</summary>
public static class Action43
{
    /// <summary>The action byte <c>43</c> that starts every packet of this group.</summary>
    public const byte Code = 43;

    /// <summary>Client to server: asks another guild for an alliance.</summary>
    public const byte Request = 1;

    /// <summary>Client to server: answers an alliance request.</summary>
    public const byte Answer = 2;

    /// <summary>Client to server: leaves the alliance.</summary>
    public const byte Leave = 3;

    /// <summary>Server to client: the alliance's guilds.</summary>
    public const byte List = 4;

    /// <summary>Server to client: a guild left the alliance.</summary>
    public const byte GuildLeft = 5;
}
