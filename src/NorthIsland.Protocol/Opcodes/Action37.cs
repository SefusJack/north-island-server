namespace NorthIsland.Protocol.Opcodes;

/// <summary>Sub codes of action <c>37</c>: the server address echo and PvP rank displays.</summary>
public static class Action37
{
    /// <summary>The action byte <c>37</c> that starts every packet of this group.</summary>
    public const byte Code = 37;

    /// <summary>Client to server: the server address echo.</summary>
    public const byte ServerIpEcho = 1;

    /// <summary>Server to client: the PvP win count shown above a player's name.</summary>
    public const byte PvpVictories = 1;

    /// <summary>Server to client: a player's PvP rank, 1 to 3.</summary>
    public const byte PvpRank = 2;
}
