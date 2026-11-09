using NorthIsland.Protocol.IO;
using NorthIsland.Protocol.Opcodes;

namespace NorthIsland.Protocol.Messages.Player;

/// <summary>Builders for the friend and mail packets of action 14, all server to client.</summary>
public static class FriendMessages
{
    /// <summary>Tells a player that another asks to become friends.</summary>
    /// <param name="requesterId">Id of the asking player.</param>
    /// <returns>The packet.</returns>
    public static byte[] Request(int requesterId) => new PacketWriter(Action14.Code, Action14.FriendRequest).I32(requesterId).ToArray();

    /// <summary>Answers the player who asked for a friendship.</summary>
    /// <param name="id">Id of the answering player.</param>
    /// <param name="answer">1 accepted, 2 refused, 3 no response, anything else busy.</param>
    /// <returns>The packet.</returns>
    public static byte[] Answer(int id, byte answer) => new PacketWriter(Action14.Code, Action14.FriendAnswer).I32(id).U8(answer).ToArray();

    /// <summary>Sends the block list reply: a code, then the blocked players.</summary>
    /// <param name="code">The reply code.</param>
    /// <param name="entries">Id and name of each blocked player.</param>
    /// <returns>The packet.</returns>
    public static byte[] Blocklist(byte code, IEnumerable<(int Id, string Name)> entries)
    {
        var w = new PacketWriter(Action14.Code, Action14.BlocklistReply).U8(code);
        foreach (var (id, name) in entries)
            w.I32(id).PStr8(name);
        return w.ToArray();
    }

    /// <summary>Tells a player that another blocked them.</summary>
    /// <param name="action">The action byte sent.</param>
    /// <returns>The packet.</returns>
    public static byte[] BlockedBy(byte action) => new PacketWriter(Action14.Code, Action14.BlockedBy).U8(action).ToArray();

    /// <summary>Removes a friend from the friend list.</summary>
    /// <param name="id">Id of the friend.</param>
    /// <returns>The packet.</returns>
    public static byte[] Removed(int id) => new PacketWriter(Action14.Code, Action14.FriendRemoved).I32(id).ToArray();

    /// <summary>Adds or updates a friend list entry.</summary>
    /// <param name="f">The friend.</param>
    /// <returns>The packet.</returns>
    public static byte[] Entry(FriendInfo f) =>
        new PacketWriter(Action14.Code, Action14.FriendEntry).I32(f.Id).PStr8(f.Name)
            .U8(f.Level).U8(f.Job).U8(f.Reborn).U8(f.Element).U8(f.BodyType).U8(f.Head).I32(f.Colour1).I32(f.Colour2).PStr8(f.Nickname).ToArray();

    /// <summary>Sends the online state of friends.</summary>
    /// <param name="friends">Id and online flag of each friend.</param>
    /// <returns>The packet.</returns>
    public static byte[] OnlineStates(IEnumerable<(int Id, bool Online)> friends)
    {
        var w = new PacketWriter(Action14.Code, Action14.OnlineStates);
        foreach (var (id, online) in friends)
            w.I32(id).U8(online ? (byte)0 : (byte)3);
        return w.ToArray();
    }

    /// <summary>Tells the client that its friend list is full.</summary>
    /// <returns>The packet.</returns>
    public static byte[] ListFull() => new PacketWriter(Action14.Code, Action14.FriendListFull).ToArray();

    /// <summary>Delivers a mail.</summary>
    /// <param name="senderId">Id of the sender.</param>
    /// <param name="time">Send time as an OLE automation date.</param>
    /// <param name="text">The mail text.</param>
    /// <returns>The packet.</returns>
    public static byte[] Mail(int senderId, double time, string text) =>
        new PacketWriter(Action14.Code, Action14.Mail).I32(senderId).F64(time).RawString(text).ToArray();
}
