namespace NorthIsland.Protocol.Opcodes;

/// <summary>Action id 45: marriage (application, wedding, spouse card, pet visibility and divorce).</summary>
public static class Action45
{
    /// <summary>Action id of this packet group, 45.</summary>
    public const byte Code = 45;

    /// <summary>Server to client: shows the marriage application form; the body holds the partner's id.</summary>
    public const byte Application = 1;

    /// <summary>Client to server: asks for the spouse card to be sent again.</summary>
    public const byte Refresh = 4;

    /// <summary>Server to client: the partner's spouse card record.</summary>
    public const byte SpouseInfo = 3;

    /// <summary>Server to client: whether the spouse is online (body 0 = online, 3 = not).</summary>
    public const byte SpouseOnline = 6;

    /// <summary>Client to server: the player walked, so their pets are shown again.</summary>
    public const byte PetFlagClear = 8;

    /// <summary>Server to client: the pets of the player whose id follows are not drawn.</summary>
    public const byte PetHide = 14;

    /// <summary>Server to client: the pets of the player whose id follows are drawn again.</summary>
    public const byte PetShow = 15;

    /// <summary>
    /// Client to server: a mail to the spouse written on the marriage card (same value as <c>SpouseOnline</c>, which goes the other way).
    /// </summary>
    public const byte SpouseMail = 6;

    /// <summary>Server to client: the id of the player's spouse.</summary>
    public const byte SpouseId = 9;

    /// <summary>Sub code 9 for a divorce answer; no code uses it (the answer the server handles is <c>DivorceFormAnswer</c>).</summary>
    public const byte DivorceAnswer = 9;

    /// <summary>Client to server: the answer to a marriage application form.</summary>
    public const byte ApplicationAnswer = 11;

    /// <summary>Client to server: the answer to the divorce form.</summary>
    public const byte DivorceFormAnswer = 12;

    /// <summary>Server to client: result code of a marriage step; it also hides the application form.</summary>
    public const byte Result = 20;

    /// <summary>Server to client: the current wedding step with the groom's and the bride's ids.</summary>
    public const byte WeddingStep = 21;

    /// <summary>Server to client: locks (1) or unlocks (0) the player's input.</summary>
    public const byte InputLock = 22;

    /// <summary>Server to client: opens the divorce form for the other player, flagged when the spouse has already applied.</summary>
    public const byte DivorceForm = 23;

    /// <summary>Server to client: result code of a divorce step; it also hides the divorce form.</summary>
    public const byte DivorceResult = 24;
}
