using NorthIsland.Protocol.IO;
using NorthIsland.Protocol.Opcodes;

namespace NorthIsland.Protocol.Messages.World;

/// <summary>Builders for the team packets (action 13) and related packets.</summary>
public static class TeamMessages
{
    /// <summary>Builds the <c>13/1</c> packet that asks a leader to let a player join.</summary>
    /// <param name="requesterId">Id of the asking player.</param>
    /// <returns>The packet.</returns>
    public static byte[] JoinRequest(int requesterId) => new PacketWriter(Action13.Code, Action13.JoinRequest).I32(requesterId).ToArray();

    /// <summary>Builds the <c>13/15</c> packet (type 1) that offers the team lead to a member.</summary>
    /// <param name="leaderId">Id of the offering leader.</param>
    /// <returns>The packet.</returns>
    public static byte[] LeaderOffer(int leaderId) => new PacketWriter(Action13.Code, Action13.Leader).U8(1).I32(leaderId).ToArray();

    /// <summary>Builds the <c>13/15</c> packet (type 2) with the answer to a lead offer.</summary>
    /// <param name="code">Answer code; 2 appends the member id.</param>
    /// <param name="memberId">Id of the member, written when the code is 2.</param>
    /// <returns>The packet.</returns>
    public static byte[] LeaderAnswer(byte code, int memberId = 0)
    {
        var w = new PacketWriter(Action13.Code, Action13.Leader).U8(2).U8(code);
        if (code == 2)
            w.I32(memberId);
        return w.ToArray();
    }

    /// <summary>Builds the <c>13/15</c> packet (type 3) that announces a new team leader.</summary>
    /// <param name="newLeader">Id of the new leader.</param>
    /// <param name="oldLeader">Id of the old leader.</param>
    /// <returns>The packet.</returns>
    public static byte[] LeaderChanged(int newLeader, int oldLeader) =>
        new PacketWriter(Action13.Code, Action13.Leader).U8(3).I32(newLeader).I32(oldLeader).ToArray();

    /// <summary>Builds the <c>8/3</c> packet with an attribute of a teammate.</summary>
    /// <param name="playerId">Id of the teammate.</param>
    /// <param name="attrId">Attribute id.</param>
    /// <param name="value">Attribute value; a negative value is sent with a sign byte of 2.</param>
    /// <param name="value2">Second value.</param>
    /// <returns>The packet.</returns>
    public static byte[] MateAttribute(int playerId, byte attrId, int value, int value2 = 0) =>
        new PacketWriter(Action8.Code, Action8.WorldPlayerStat)
            .I32(playerId)
            .U8(attrId)
            .U8((byte)(value < 0 ? 2 : 1))
            .I32(Math.Abs(value))
            .I32(value2)
            .ToArray();

    /// <summary>Builds the <c>13/11</c> packet that shows the counsellor fan icon on players.</summary>
    /// <param name="ids">Ids of the players.</param>
    /// <returns>The packet.</returns>
    public static byte[] FanIcon(IEnumerable<int> ids)
    {
        var w = new PacketWriter(Action13.Code, Action13.FanIcon);
        foreach (var id in ids)
            w.I32(id);
        return w.ToArray();
    }

    /// <summary>Builds the <c>13/12</c> packet that hides the counsellor fan icon on players.</summary>
    /// <param name="ids">Ids of the players.</param>
    /// <returns>The packet.</returns>
    public static byte[] HideFanIcon(IEnumerable<int> ids)
    {
        var w = new PacketWriter(Action13.Code, Action13.HideFanIcon);
        foreach (var id in ids)
            w.I32(id);
        return w.ToArray();
    }

    /// <summary>Builds the <c>13/2</c> packet that tells the player the other side cannot join or invite now.</summary>
    /// <returns>The packet.</returns>
    public static byte[] Busy() => new PacketWriter(Action13.Code, Action13.Busy).U8(1).ToArray();

    /// <summary>Builds the <c>13/3</c> packet with the leader's answer to a join request.</summary>
    /// <param name="answer">Answer byte (1 accepts).</param>
    /// <param name="leaderId">Id of the answering leader.</param>
    /// <returns>The packet.</returns>
    public static byte[] JoinAnswer(byte answer, int leaderId) => new PacketWriter(Action13.Code, Action13.JoinAnswer).U8(answer).I32(leaderId).ToArray();

    /// <summary>Builds the <c>13/4</c> packet that reports a player leaving or the team ending.</summary>
    /// <param name="id">Id of the leader (the team ends) or of the member who left.</param>
    /// <returns>The packet.</returns>
    public static byte[] Left(int id) => new PacketWriter(Action13.Code, Action13.Left).I32(id).ToArray();

    /// <summary>Builds the <c>13/5</c> packet that reports a member joining a team.</summary>
    /// <param name="leaderId">Id of the leader.</param>
    /// <param name="memberId">Id of the new member.</param>
    /// <returns>The packet.</returns>
    public static byte[] Joined(int leaderId, int memberId) => new PacketWriter(Action13.Code, Action13.Joined).I32(leaderId).I32(memberId).ToArray();

    /// <summary>Builds the <c>13/6</c> packet with the teams in the scene.</summary>
    /// <param name="teams">Leader id and member ids of each team.</param>
    /// <returns>The packet.</returns>
    public static byte[] SceneTeams(IEnumerable<(int Leader, IReadOnlyList<int> Members)> teams)
    {
        var w = new PacketWriter(Action13.Code, Action13.SceneTeams);
        foreach (var (leader, members) in teams)
        {
            w.I32(leader).U8((byte)members.Count);
            foreach (var m in members)
                w.I32(m);
        }
        return w.ToArray();
    }

    /// <summary>Builds the <c>13/9</c> packet that invites a player to a team.</summary>
    /// <param name="inviterId">Id of the inviting player.</param>
    /// <returns>The packet.</returns>
    public static byte[] Invite(int inviterId) => new PacketWriter(Action13.Code, Action13.Invite).I32(inviterId).ToArray();

    /// <summary>Builds the <c>13/10</c> packet with the answer to an invitation.</summary>
    /// <param name="answer">Answer byte (1 accepted, 2 refused, 3 no response).</param>
    /// <param name="inviteeId">Id of the invited player.</param>
    /// <returns>The packet.</returns>
    public static byte[] InviteAnswer(byte answer, int inviteeId) => new PacketWriter(Action13.Code, Action13.InviteAnswer).U8(answer).I32(inviteeId).ToArray();
}
