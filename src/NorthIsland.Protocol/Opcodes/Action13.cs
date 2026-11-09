namespace NorthIsland.Protocol.Opcodes;

/// <summary>Action 13: teams.</summary>
public static class Action13
{
    /// <summary>Action code 13.</summary>
    public const byte Code = 13;

    /// <summary>Client to server: asks a leader to let the sender join; server to client: the request arrives at the leader.</summary>
    public const byte JoinRequest = 1;

    /// <summary>Server to client: the join or invite cannot be done now.</summary>
    public const byte Busy = 2;

    /// <summary>Client to server: the leader's answer to a join request; server to client: the answer reaches the requester.</summary>
    public const byte JoinAnswer = 3;

    /// <summary>Client to server: the sender leaves the team; server to client: a player left or the team ended.</summary>
    public const byte Left = 4;

    /// <summary>Server to client: a member joined a team.</summary>
    public const byte Joined = 5;

    /// <summary>Server to client: the teams in the scene.</summary>
    public const byte SceneTeams = 6;

    /// <summary>Client to server: invites a player to the sender's team.</summary>
    public const byte InviteRequest = 7;

    /// <summary>Client to server: the answer to an invitation.</summary>
    public const byte InviteReply = 8;

    /// <summary>Client to server: the leader removes a member.</summary>
    public const byte Kick = 9;

    /// <summary>Server to client: the invitation arrives at the invited player.</summary>
    public const byte Invite = 9;

    /// <summary>Server to client: the invited player's answer arrives at the inviter.</summary>
    public const byte InviteAnswer = 10;

    /// <summary>Client to server: marks a member as counsellor.</summary>
    public const byte CounsellorOn = 5;

    /// <summary>Client to server: removes the counsellor mark from a member.</summary>
    public const byte CounsellorOff = 6;

    /// <summary>Client to server: offers or answers the team lead.</summary>
    public const byte LeaderChange = 10;

    /// <summary>Server to client: shows the counsellor fan icon on players.</summary>
    public const byte FanIcon = 11;

    /// <summary>Server to client: hides the counsellor fan icon on players.</summary>
    public const byte HideFanIcon = 12;

    /// <summary>Server to client: lead offer, answer and change.</summary>
    public const byte Leader = 15;
}
