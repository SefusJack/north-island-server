namespace NorthIsland.Server.Data;

/// <summary>A friend in a character's friend list.</summary>
public sealed class FriendEntry
{
    /// <summary>The most friends a list holds.</summary>
    public const int MaxFriends = 50;

    /// <summary>The friend's first colour.</summary>
    public int Colour1 { get; set; }

    /// <summary>The friend's second colour.</summary>
    public int Colour2 { get; set; }

    /// <summary>The friend's player id.</summary>
    public int Id { get; set; }

    /// <summary>The friend's name.</summary>
    public string Name { get; set; } = "";

    /// <summary>The friend's level.</summary>
    public byte Level { get; set; }

    /// <summary>The friend's job.</summary>
    public byte Job { get; set; }

    /// <summary>How many times the friend was reborn.</summary>
    public byte Reborn { get; set; }

    /// <summary>The friend's element.</summary>
    public byte Element { get; set; }

    /// <summary>The friend's body type.</summary>
    public byte BodyType { get; set; }

    /// <summary>The friend's head.</summary>
    public byte Head { get; set; }

    /// <summary>The friend's nickname.</summary>
    public string Nickname { get; set; } = "";
}
