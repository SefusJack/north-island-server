namespace NorthIsland.Protocol.Messages.Player;

/// <summary>One friend list entry.</summary>
/// <param name="Id">The friend's id.</param>
/// <param name="Name">The friend's name.</param>
/// <param name="Level">The friend's level.</param>
/// <param name="Job">The friend's job.</param>
/// <param name="Reborn">How many times the friend was reborn.</param>
/// <param name="Element">The friend's element.</param>
/// <param name="BodyType">Body type of the friend's model.</param>
/// <param name="Head">Head of the friend's model.</param>
/// <param name="Nickname">The friend's nickname.</param>
/// <param name="Colour1">First colour value of the friend's model.</param>
/// <param name="Colour2">Second colour value of the friend's model.</param>
public sealed record FriendInfo(
    int Id,
    string Name,
    byte Level,
    byte Job,
    byte Reborn,
    byte Element,
    byte BodyType,
    byte Head,
    string Nickname,
    int Colour1 = 0,
    int Colour2 = 0);
