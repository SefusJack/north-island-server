using NorthIsland.Protocol.Messages.Player;

namespace NorthIsland.Server.Features.Guilds;

/// <summary>A guild member as the guild window shows them; the data is copied from the character when they log in.</summary>
public sealed class GuildMember
{
    /// <summary>The member's login id.</summary>
    public int Id { get; set; }

    /// <summary>The member's name.</summary>
    public string Name { get; set; } = "";

    /// <summary>The member's level.</summary>
    public byte Level { get; set; }

    /// <summary>The member's job.</summary>
    public byte Job { get; set; }

    /// <summary>How many times the member was reborn.</summary>
    public byte Reborn { get; set; }

    /// <summary>The member's element.</summary>
    public byte Element { get; set; }

    /// <summary>Body type of the member's model.</summary>
    public byte BodyType { get; set; }

    /// <summary>Head of the member's model.</summary>
    public byte Head { get; set; }

    /// <summary>First colour value of the member's model.</summary>
    public int Colour1 { get; set; }

    /// <summary>Second colour value of the member's model.</summary>
    public int Colour2 { get; set; }

    /// <summary>The member's nickname.</summary>
    public string Nickname { get; set; } = "";

    /// <summary>The member's contribution points.</summary>
    public int Contribution { get; set; }

    /// <summary>The member's five rights as 0 or 1 bytes; the first allows changing the badge and the rights, the second allows inviting.</summary>
    public byte[] Rights { get; set; } = new byte[5];

    /// <summary>Copies the name, level, job, reborn count, element, look and nickname from a character.</summary>
    /// <param name="c">The character.</param>
    public void Refresh(Character c)
    {
        Name = c.Name;
        Level = c.Level;
        Job = c.Job;
        Reborn = c.RebornCount;
        Element = c.Element;
        BodyType = c.BodyType;
        Head = c.Head;
        Colour1 = c.Colour1;
        Colour2 = c.Colour2;
        Nickname = c.Nickname;
    }

    /// <summary>The member as the guild info packet carries them.</summary>
    /// <returns>The packet record.</returns>
    public GuildMemberInfo Info() =>
        new(Id, Name, Level, Job, Reborn, Element, BodyType, Head, Colour1, Colour2, Nickname, Contribution, Rights);
}
