namespace NorthIsland.Protocol.Opcodes;

/// <summary>Action id 51: skills cast on the map and fighter attributes in a battle.</summary>
public static class Action51
{
    /// <summary>Action id of this packet group, 51.</summary>
    public const byte Code = 51;

    /// <summary>Client to server: casts a skill on the map, outside a battle.</summary>
    public const byte CastOnMap = 1;

    /// <summary>Server to client: the battle time is reset.</summary>
    public const byte BattleTimeReset = 2;

    /// <summary>Server to client: the attributes of the fighters in a battle.</summary>
    public const byte FighterAttributes = 1;
}
