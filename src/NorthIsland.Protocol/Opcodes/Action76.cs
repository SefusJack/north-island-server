namespace NorthIsland.Protocol.Opcodes;

/// <summary>Action id 76: the maid in the house.</summary>
public static class Action76
{
    /// <summary>Action id of this packet group, 76.</summary>
    public const byte Code = 76;

    /// <summary>Client to server: a bank order to the maid.</summary>
    public const byte Bank = 1;

    /// <summary>Client to server: a pet order to the maid.</summary>
    public const byte Pets = 2;

    /// <summary>Client to server: changes the maid's clothes; server to client: the new costume.</summary>
    public const byte Costume = 3;

    /// <summary>Client to server: the maid menu was closed; server to client: hides it.</summary>
    public const byte Hide = 250;
}
