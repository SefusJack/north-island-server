namespace NorthIsland.Protocol.Opcodes;

/// <summary>The kinds of the experience potion packet <c>5/17</c> (server to client); the kind is the first byte of its body.</summary>
public static class ExpPotionKind
{
    /// <summary>Kind 1; nothing in the server sends it.</summary>
    public const byte Icon = 1;

    /// <summary>Kind 2: the potion was refused; the next byte is the reason code.</summary>
    public const byte Refused = 2;

    /// <summary>Kind 3: whether a potion is active, the minutes left and the rate.</summary>
    public const byte Remaining = 3;

    /// <summary>Kind 4: the potion ran out; the next byte says whether another one is still active.</summary>
    public const byte Expired = 4;
}
