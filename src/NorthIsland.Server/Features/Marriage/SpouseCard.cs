using NorthIsland.Protocol.Messages.Player;

namespace NorthIsland.Server.Features.Marriage;

/// <summary>A snapshot of a married player, shown on the partner's card and kept for when the player is offline.</summary>
public sealed class SpouseCard
{
    /// <summary>Login id of the player.</summary>
    public int Id { get; set; }

    /// <summary>Character name.</summary>
    public string Name { get; set; } = "";

    /// <summary>Nickname.</summary>
    public string Nickname { get; set; } = "";

    /// <summary>Body type.</summary>
    public byte BodyType { get; set; }

    /// <summary>Head.</summary>
    public byte Head { get; set; }

    /// <summary>Sprite.</summary>
    public byte Sprite { get; set; }

    /// <summary>Element.</summary>
    public byte Element { get; set; }

    /// <summary>Item ids worn in equipment slots 1 to 6; 0 for an empty slot.</summary>
    public ushort[] Equipment { get; set; } = new ushort[6];

    /// <summary>First colour.</summary>
    public int Colour1 { get; set; }

    /// <summary>Second colour.</summary>
    public int Colour2 { get; set; }

    /// <summary>Hit points.</summary>
    public int Hp { get; set; }

    /// <summary>Spirit points.</summary>
    public int Sp { get; set; }

    /// <summary>Strength.</summary>
    public ushort Strength { get; set; }

    /// <summary>Constitution.</summary>
    public ushort Constitution { get; set; }

    /// <summary>Intelligence.</summary>
    public ushort Intelligence { get; set; }

    /// <summary>Wisdom.</summary>
    public ushort Wisdom { get; set; }

    /// <summary>Agility.</summary>
    public ushort Agility { get; set; }

    /// <summary>Level.</summary>
    public byte Level { get; set; }

    /// <summary>Experience points.</summary>
    public int Exp { get; set; }

    /// <summary>Reborn count.</summary>
    public byte Reborn { get; set; }

    /// <summary>Job.</summary>
    public byte Job { get; set; }

    /// <summary>Map the character was last on.</summary>
    public ushort MapId { get; set; }

    /// <summary>When the card was last refreshed from the character (UTC).</summary>
    public DateTime LastSeenUtc { get; set; }

    /// <summary>Copies the character's current look, stats and map into the card and stamps the time.</summary>
    /// <param name="c">The player's character.</param>
    public void Refresh(Character c)
    {
        Name = c.Name;
        Nickname = c.Nickname;
        (BodyType, Head, Sprite, Element) = (c.BodyType, c.Head, c.Sprite, c.Element);
        Equipment = [.. Enumerable.Range(1, 6).Select(slot => c.Equipment.Find(e => e.Slot == slot)?.ItemId ?? 0)];
        (Colour1, Colour2, Hp, Sp) = (c.Colour1, c.Colour2, c.Hp, c.Sp);
        (Strength, Constitution, Intelligence, Wisdom, Agility) = (c.Strength, c.Constitution, c.Intelligence, c.Wisdom, c.Agility);
        (Level, Exp, Reborn, Job, MapId) = (c.Level, c.Exp, c.RebornCount, c.Job, c.MapId);
        LastSeenUtc = GameClock.UtcNow;
    }

    /// <summary>Builds the card as the client receives it; hit and spirit points are clamped to 16 bits.</summary>
    /// <param name="guildName">The player's guild name, or empty for none.</param>
    /// <returns>The card for the client.</returns>
    public SpouseCardInfo Info(string guildName) => new()
    {
        Id = Id,
        BodyType = BodyType,
        Head = Head,
        Sprite = Sprite,
        Element = Element,
        Equipment = Equipment,
        Colour1 = Colour1,
        Colour2 = Colour2,
        Hp = (ushort)Math.Clamp(Hp, 0, ushort.MaxValue),
        Sp = (ushort)Math.Clamp(Sp, 0, ushort.MaxValue),
        Strength = Strength,
        Constitution = Constitution,
        Intelligence = Intelligence,
        Wisdom = Wisdom,
        Agility = Agility,
        Level = Level,
        Exp = Exp,
        Reborn = Reborn,
        Job = Job,
        MapId = MapId,
        Name = Name,
        GuildName = guildName,
        Nickname = Nickname,
    };
}
