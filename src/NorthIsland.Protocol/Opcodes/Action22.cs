namespace NorthIsland.Protocol.Opcodes;

/// <summary>Action 22: map NPCs.</summary>
public static class Action22
{
    /// <summary>Action code 22.</summary>
    public const byte Code = 22;

    /// <summary>Server to client: state of a map NPC.</summary>
    public const byte State = 1;

    /// <summary>Server to client: a map NPC walks to a place.</summary>
    public const byte Move = 2;

    /// <summary>Server to client: a map NPC stands at a place at once.</summary>
    public const byte Place = 5;

    /// <summary>Server to client: hides a map NPC for some seconds.</summary>
    public const byte Hide = 3;

    /// <summary>Server to client: a map NPC shows an emotion.</summary>
    public const byte Emotion = 7;

    /// <summary>Server to client: a map NPC plays an action.</summary>
    public const byte Action = 8;

    /// <summary>Server to client: a map NPC plays an action in a direction.</summary>
    public const byte ActionDir = 9;

    /// <summary>Server to client: hides a map NPC for a number of seconds.</summary>
    public const byte HideFor = 10;

    /// <summary>Server to client: minimap sign of an NPC, door or mine.</summary>
    public const byte Sign = 12;

    /// <summary>Server to client: shows a map NPC for a number of seconds.</summary>
    public const byte ShowFor = 11;

    /// <summary>Server to client: a list of NPC state records.</summary>
    public const byte StateList = 4;
}
