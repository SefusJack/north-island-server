namespace NorthIsland.Protocol.Opcodes;

/// <summary>Action 14: friends and mail.</summary>
public static class Action14
{
    /// <summary>Action code 14.</summary>
    public const byte Code = 14;

    /// <summary>Client to server: sends a mail; server to client: a mail arrives.</summary>
    public const byte Mail = 1;

    /// <summary>Friend request, both directions.</summary>
    public const byte FriendRequest = 2;

    /// <summary>Answer to a friend request, both directions.</summary>
    public const byte FriendAnswer = 3;

    /// <summary>Client to server: deletes a friend; server to client: a friend was removed.</summary>
    public const byte FriendRemoved = 4;

    /// <summary>Client to server: blocklist add, remove or list request.</summary>
    public const byte Blocklist = 5;

    /// <summary>Server to client: result code and, for some codes, the blocklist.</summary>
    public const byte BlocklistReply = 13;

    /// <summary>Server to client: the target has blocked the sender.</summary>
    public const byte BlockedBy = 14;

    /// <summary>Result codes of the blocklist reply: 0 failed, 1 added, 2 removed, 3 full.</summary>
    public const byte BlocklistFailed = 0, BlocklistAdded = 1, BlocklistRemoved = 2, BlocklistFull = 3;

    /// <summary>Server to client: the mailbox of the target is full.</summary>
    public const byte MailboxFull = 6;

    /// <summary>Client to server: a letter written at a furniture mailbox.</summary>
    public const byte FurMail = 6;

    /// <summary>Server to client: furniture mailbox results, letters and the form.</summary>
    public const byte FurMailPacket = 16;

    /// <summary>Server to client: online states of friends.</summary>
    public const byte OnlineStates = 9;

    /// <summary>Server to client: one friend list entry.</summary>
    public const byte FriendEntry = 11;

    /// <summary>Server to client: the friend list is full.</summary>
    public const byte FriendListFull = 12;
}
