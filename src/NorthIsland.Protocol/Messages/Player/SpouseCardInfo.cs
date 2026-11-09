namespace NorthIsland.Protocol.Messages.Player;

/// <summary>What the spouse card shows of the spouse.</summary>
public sealed record SpouseCardInfo
{
    /// <summary>The spouse's id.</summary>
    public int Id { get; init; }

    /// <summary>Body type of the spouse's model.</summary>
    public byte BodyType { get; init; }

    /// <summary>Head of the spouse's model.</summary>
    public byte Head { get; init; }

    /// <summary>Sprite of the spouse's model.</summary>
    public byte Sprite { get; init; }

    /// <summary>The spouse's element.</summary>
    public byte Element { get; init; }

    /// <summary>Item ids worn, six are written (missing ones as 0).</summary>
    public ushort[] Equipment { get; init; } = new ushort[6];

    /// <summary>First colour value of the spouse's model.</summary>
    public int Colour1 { get; init; }

    /// <summary>Second colour value of the spouse's model.</summary>
    public int Colour2 { get; init; }

    /// <summary>Current HP.</summary>
    public int Hp { get; init; }

    /// <summary>Current SP.</summary>
    public ushort Sp { get; init; }

    /// <summary>Strength.</summary>
    public ushort Strength { get; init; }

    /// <summary>Constitution.</summary>
    public ushort Constitution { get; init; }

    /// <summary>Intelligence.</summary>
    public ushort Intelligence { get; init; }

    /// <summary>Wisdom.</summary>
    public ushort Wisdom { get; init; }

    /// <summary>Agility.</summary>
    public ushort Agility { get; init; }

    /// <summary>Level.</summary>
    public byte Level { get; init; }

    /// <summary>Experience points.</summary>
    public int Exp { get; init; }

    /// <summary>How many times the spouse was reborn.</summary>
    public byte Reborn { get; init; }

    /// <summary>The spouse's job.</summary>
    public byte Job { get; init; }

    /// <summary>Id of the map the spouse is on.</summary>
    public ushort MapId { get; init; }

    /// <summary>The spouse's name.</summary>
    public string Name { get; init; } = "";

    /// <summary>Name of the spouse's guild.</summary>
    public string GuildName { get; init; } = "";

    /// <summary>The spouse's nickname.</summary>
    public string Nickname { get; init; } = "";

    /// <summary>Name of the place the spouse is at.</summary>
    public string Place { get; init; } = "";
}
