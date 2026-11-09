namespace NorthIsland.Protocol.Opcodes;

/// <summary>Sub codes of action <c>39</c>: guilds, their members, posts, badges and mail.</summary>
public static class Action39
{
    /// <summary>The action byte <c>39</c> that starts every packet of this group.</summary>
    public const byte Code = 39;

    /// <summary>Both ways: creates a guild; the server answers with a result code.</summary>
    public const byte Create = 1;

    /// <summary>Client to server: invites a player into the guild.</summary>
    public const byte Invite = 2;

    /// <summary>Server to client: the guild's name, vices and members.</summary>
    public const byte Info = 2;

    /// <summary>Both ways: the answer to a guild invitation; the server sends the invitation with the inviter id.</summary>
    public const byte Answer = 3;

    /// <summary>Server to client: the outcome of an invitation for the inviter.</summary>
    public const byte InviteResult = 4;

    /// <summary>Both ways: leaves the guild; the server tells the guild which member was removed.</summary>
    public const byte Leave = 6;

    /// <summary>Both ways: removes a member; the server answers with a result and the member id.</summary>
    public const byte Kick = 7;

    /// <summary>Client to server: disbands the guild.</summary>
    public const byte Disband = 8;

    /// <summary>Server to client: a member joined the guild.</summary>
    public const byte Joined = 8;

    /// <summary>Client to server: sets the guild rules.</summary>
    public const byte Rules = 9;

    /// <summary>Server to client: the guild tags of the scene's players.</summary>
    public const byte Tags = 9;

    /// <summary>Server to client: the guild was disbanded, with a result code.</summary>
    public const byte Disbanded = 10;

    /// <summary>Server to client: the guild rules changed, with the new text.</summary>
    public const byte RulesChanged = 11;

    /// <summary>Client to server: offers a member the vice post.</summary>
    public const byte AppointVice = 10;

    /// <summary>Client to server: removes a vice.</summary>
    public const byte DeposeVice = 11;

    /// <summary>Client to server: offers a member the lord post.</summary>
    public const byte Abdicate = 12;

    /// <summary>Server to client: a member became vice.</summary>
    public const byte ViceAppointed = 12;

    /// <summary>Client to server: the answer to an offer of the lord post.</summary>
    public const byte LordAnswer = 13;

    /// <summary>Server to client: a vice was removed.</summary>
    public const byte ViceDeposed = 13;

    /// <summary>Client to server: the answer to an offer of the vice post.</summary>
    public const byte ViceAnswer = 14;

    /// <summary>Server to client: the guild has a new lord.</summary>
    public const byte NewLord = 14;

    /// <summary>Server to client: a post change was refused, with a reason code.</summary>
    public const byte PositionError = 15;

    /// <summary>Server to client: the player is offered the vice post.</summary>
    public const byte ViceOffer = 18;

    /// <summary>Server to client: the player is offered the lord post.</summary>
    public const byte LordOffer = 19;

    /// <summary>Server to client: a post offer was accepted.</summary>
    public const byte PostAccepted = 25;

    /// <summary>Server to client: an action is refused because of the Trojan War.</summary>
    public const byte WarRefused = 29;

    /// <summary>Client to server: closes or opens the alliance function.</summary>
    public const byte AllyFunction = 17;

    /// <summary>Server to client: whether the alliance function is closed or open.</summary>
    public const byte AllyFunctionState = 28;

    /// <summary>Client to server: uploads the guild's 288 byte badge.</summary>
    public const byte UploadBadge = 18;

    /// <summary>Client to server: asks for the badges the client's cache lacks, by guild id.</summary>
    public const byte BadgeRequest = 19;

    /// <summary>Server to client: the player's own guild badge changed.</summary>
    public const byte OwnBadge = 30;

    /// <summary>Server to client: the guilds that have a badge, with their versions.</summary>
    public const byte BadgeList = 31;

    /// <summary>Server to client: guild badges with their versions.</summary>
    public const byte Badges = 32;

    /// <summary>Server to client: the guild's new name.</summary>
    public const byte GuildNameChanged = 33;

    /// <summary>Server to client: a guild's badge must be replaced by a sample file.</summary>
    public const byte BadgeReset = 34;

    /// <summary>Client to server: sends a letter to guild members.</summary>
    public const byte Mail = 4;

    /// <summary>Server to client: a letter from a guild member, with sender id, time and text.</summary>
    public const byte MailReceived = 5;

    /// <summary>Client to server: sets a member's rights.</summary>
    public const byte Rights = 16;

    /// <summary>Server to client: the guild mailbox is full; no data.</summary>
    public const byte MailboxFull = 20;

    /// <summary>Server to client: the online states of the guild's members.</summary>
    public const byte MemberStates = 61;

    /// <summary>Server to client: a member's rights.</summary>
    public const byte MemberRights = 24;

    /// <summary>Server to client: opens the guild creation form; no data.</summary>
    public const byte OpenCreate = 40;

    /// <summary>Server to client: welcomes the player into the guild.</summary>
    public const byte Welcome = 62;
}
