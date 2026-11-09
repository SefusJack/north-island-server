namespace NorthIsland.Protocol.Opcodes;

/// <summary>Sub codes of action <c>32</c>: emotions and actions.</summary>
public static class Action32
{
    /// <summary>The action byte <c>32</c> that starts every packet of this group.</summary>
    public const byte Code = 32;

    /// <summary>Both ways: a player's emotion; the server forwards the player id and the emotion.</summary>
    public const byte Emotion = 1;

    /// <summary>Both ways: a player's action animation; the server forwards the player id and the frame.</summary>
    public const byte Action = 2;

    /// <summary>Server to client: shows an emotion over a player.</summary>
    public const byte ShowEmotion = 3;

    /// <summary>Client to server: uses the Yes-No Board from a bag slot with a choice.</summary>
    public const byte YesNo = 4;

    /// <summary>Client to server: stops the current action.</summary>
    public const byte StopAction = 3;
}
