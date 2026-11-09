namespace NorthIsland.Protocol.Opcodes;

/// <summary>Sub codes of action 87, the switch between the normal server and the PvP server.</summary>
public static class Action87
{
    /// <summary>The action number, 87.</summary>
    public const byte Code = 87;

    /// <summary>
    /// Server to client: sub code of the packet that tells the client to log in again at another server; its mode byte is <see cref="ToPvp"/> or <see
    /// cref="ToHome"/>.
    /// </summary>
    public const byte Relogin = 1;

    /// <summary>Mode byte of <c>87/1</c>: the client logs in again at the PvP server.</summary>
    public const byte ToPvp = 1;

    /// <summary>Mode byte of <c>87/1</c>: the client goes back to the normal server.</summary>
    public const byte ToHome = 2;
}
