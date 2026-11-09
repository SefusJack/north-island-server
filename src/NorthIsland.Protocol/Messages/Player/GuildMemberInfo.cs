namespace NorthIsland.Protocol.Messages.Player;

/// <summary>One guild member as the guild info packet carries it.</summary>
/// <param name="Id">The member's id.</param>
/// <param name="Name">The member's name.</param>
/// <param name="Level">The member's level.</param>
/// <param name="Job">The member's job.</param>
/// <param name="Reborn">How many times the member was reborn.</param>
/// <param name="Element">The member's element.</param>
/// <param name="BodyType">Body type of the member's model.</param>
/// <param name="Head">Head of the member's model.</param>
/// <param name="Colour1">First colour value of the member's model.</param>
/// <param name="Colour2">Second colour value of the member's model.</param>
/// <param name="Nickname">The member's nickname.</param>
/// <param name="Contribution">The member's contribution points.</param>
/// <param name="Rights">The member's rights, five bytes (missing ones are written as 0).</param>
public sealed record GuildMemberInfo(
    int Id,
    string Name,
    byte Level,
    byte Job,
    byte Reborn,
    byte Element,
    byte BodyType,
    byte Head,
    int Colour1,
    int Colour2,
    string Nickname,
    int Contribution,
    byte[] Rights);
