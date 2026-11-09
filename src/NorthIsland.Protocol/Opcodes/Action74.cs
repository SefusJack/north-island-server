namespace NorthIsland.Protocol.Opcodes;

/// <summary>Action id 74: the GPS device.</summary>
public static class Action74
{
    /// <summary>Action id of this packet group, 74.</summary>
    public const byte Code = 74;

    /// <summary>Client to server: records the current spot on a GPS point; server to client: the point's position.</summary>
    public const byte Mark = 1;

    /// <summary>Client to server: teleports to a GPS point; server to client: the arrival reply.</summary>
    public const byte Teleport = 2;

    /// <summary>Dreaming pill state, both directions (client click and server state).</summary>
    public const byte Dreaming = 3;

    /// <summary>Client to server: upgrades the GPS device.</summary>
    public const byte Upgrade = 4;
}
