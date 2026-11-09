using System.Buffers.Binary;
using Microsoft.Extensions.Logging;
using NorthIsland.Protocol;
using NorthIsland.Protocol.IO;
using NorthIsland.Protocol.Messages.World;
using NorthIsland.Protocol.Opcodes;

namespace NorthIsland.Server.Features.Social;

/// <summary>Handles teams: joining, invitations, leaving, kicking, the lead and the counsellor mark.</summary>
/// <param name="world">World state, with the team registry.</param>
/// <param name="vitals">Shares team member vitals.</param>
/// <param name="logger">Logger.</param>
public sealed class TeamHandlers(
    WorldState world,
    TeamVitals vitals,
    ILogger<TeamHandlers> logger)
{
    /// <summary>Registers the vitals and shows counsellor icons to players who meet.</summary>
    internal void Start()
    {
        vitals.Register();
        world.PlayersMet += (a, b) =>
        {
            if (a.Team.Counsellor && a.Account is not null)
                b.Send(TeamMessages.FanIcon([a.Account.LoginId]));
            if (b.Team.Counsellor && b.Account is not null)
                a.Send(TeamMessages.FanIcon([b.Account.LoginId]));
        };
    }

    /// <summary>Handles an offer of the lead, its answer, or its acceptance.</summary>
    /// <param name="session">The player's session.</param>
    /// <param name="body">The packet body: the operation byte and its data.</param>
    internal ValueTask OnLeaderChange(GameSession session, ReadOnlyMemory<byte> body)
    {
        if (session.State != SessionState.InWorld || session.Account is null || body.Length < 1)
            return ValueTask.CompletedTask;
        var myId = session.Account.LoginId;
        switch (body.Span[0])
        {
            case 1 when body.Length >= 5: // 13/10/1: the leader offers the lead to a team member
            {
                var memberId = BinaryPrimitives.ReadInt32LittleEndian(body.Span[1..]);
                if (!world.Teams.IsLeader(myId) || !world.Teams.MembersOf(myId).Contains(memberId) || world.SessionOf(memberId) is not { } member)
                    return ValueTask.CompletedTask;
                if (member.Battle is not null || session.Battle is not null)
                {
                    session.Send(TeamMessages.LeaderAnswer(5));
                    return ValueTask.CompletedTask;
                }
                member.Team.PendingLeaderOffer = myId;
                member.Send(TeamMessages.LeaderOffer(myId));
                break;
            }
            case 2 when body.Length >= 2: // 13/10/2: the member refuses or does not answer the offer
                if (session.Team.PendingLeaderOffer is var offerer && offerer != 0)
                {
                    session.Team.PendingLeaderOffer = 0;
                    if (world.SessionOf(offerer) is { } leader)
                        leader.Send(TeamMessages.LeaderAnswer(body.Span[1] == 2 ? (byte)2 : (byte)3, myId));
                }
                break;
            case 3: // 13/10/3: the member accepts, the lead changes
                if (session.Team.PendingLeaderOffer is var from && from != 0)
                {
                    session.Team.PendingLeaderOffer = 0;
                    if (world.Teams.Promote(from, myId))
                    {
                        if (world.SessionOf(from) is { } old && old.ActiveCharacter is { } oc && session.ActiveCharacter is { } nc)
                            (oc.X, oc.Y) = (nc.X, nc.Y);
                        world.SendToMapAndTeam(session, TeamMessages.LeaderChanged(newLeader: myId, oldLeader: from), world.Teams.TeamOf(myId));
                        logger.LogInformation(
                            "[{Session}] {Name} takes over the lead of the team from {From}",
                            session.Id,
                            session.ActiveCharacter?.Name,
                            from);
                    }
                }
                break;
        }
        return ValueTask.CompletedTask;
    }

    /// <summary>Marks or unmarks a team member as counsellor (only the leader may).</summary>
    /// <param name="session">The leader's session.</param>
    /// <param name="body">The packet body: the member's login id.</param>
    /// <param name="on">True to mark, false to unmark.</param>
    internal ValueTask OnCounsellor(GameSession session, ReadOnlyMemory<byte> body, bool on)
    {
        if (session.State != SessionState.InWorld || session.Account is null || body.Length < 4)
            return ValueTask.CompletedTask;
        var memberId = BinaryPrimitives.ReadInt32LittleEndian(body.Span);
        if (!world.Teams.IsLeader(session.Account.LoginId)
            || !world.Teams.MembersOf(session.Account.LoginId).Contains(memberId)
            || world.SessionOf(memberId) is not { } member)
            return ValueTask.CompletedTask;
        member.Team.Counsellor = on;
        world.SendToMapAndTeam(session, on ? TeamMessages.FanIcon([memberId]) : TeamMessages.HideFanIcon([memberId]), world.Teams.TeamOf(memberId));
        return ValueTask.CompletedTask;
    }

    /// <summary>Handles a request to join another player's team and passes it to that team's leader.</summary>
    /// <param name="session">The asker's session.</param>
    /// <param name="body">The packet body: the leader's login id.</param>
    internal ValueTask OnJoinRequest(GameSession session, ReadOnlyMemory<byte> body)
    {
        if (OtherOf(session, body, 0) is not (var myId, var leader, var leaderId))
            return ValueTask.CompletedTask;
        if (world.Teams.LeaderOf(myId) != 0 || world.Teams.LeaderOf(leaderId) is var l && l != 0 && l != leaderId
            || world.Teams.MembersOf(leaderId).Count >= Teams.MaxMembers || leader.Battle is not null)
        {
            session.Send(TeamMessages.Busy());
            return ValueTask.CompletedTask;
        }
        if (RefusesParties(session, leader))
            return ValueTask.CompletedTask;
        leader.Team.PendingJoin = myId;
        leader.Send(TeamMessages.JoinRequest(myId));
        logger.LogInformation("[{Session}] {Name} asks to join {Leader}'s team", session.Id, session.ActiveCharacter!.Name, leader.ActiveCharacter!.Name);
        return ValueTask.CompletedTask;
    }

    /// <summary>Handles the leader's answer to a join request; on acceptance the asker joins.</summary>
    /// <param name="session">The leader's session.</param>
    /// <param name="body">The packet body: the answer byte, then the asker's login id.</param>
    internal ValueTask OnJoinAnswer(GameSession session, ReadOnlyMemory<byte> body)
    {
        if (body.Length < 5 || OtherOf(session, body, 1) is not (var myId, var requester, var requesterId))
            return ValueTask.CompletedTask;
        var answer = body.Span[0];
        if (session.Team.PendingJoin != requesterId)
            return ValueTask.CompletedTask;
        session.Team.PendingJoin = 0;
        requester.Send(TeamMessages.JoinAnswer(answer, myId));
        if (answer == 1)
            Join(session, myId, requester, requesterId);
        return ValueTask.CompletedTask;
    }

    /// <summary>Handles an invitation to a team and passes it on to the target.</summary>
    /// <param name="session">The inviter's session.</param>
    /// <param name="body">The packet body: the target's login id.</param>
    internal ValueTask OnInvite(GameSession session, ReadOnlyMemory<byte> body)
    {
        if (OtherOf(session, body, 0) is not (var myId, var target, var targetId))
            return ValueTask.CompletedTask;
        var myLeader = world.Teams.LeaderOf(myId);
        if (myLeader != 0 && myLeader != myId || world.Teams.LeaderOf(targetId) != 0
            || world.Teams.MembersOf(myId).Count >= Teams.MaxMembers || target.Battle is not null)
        {
            session.Send(TeamMessages.Busy());
            return ValueTask.CompletedTask;
        }
        if (RefusesParties(session, target))
            return ValueTask.CompletedTask;
        if (BlocklistHandlers.Blocks(target.ActiveCharacter, myId))
        {
            BlocklistHandlers.Refuse(session, 4);
            return ValueTask.CompletedTask;
        }
        target.Team.PendingInvite = myId;
        target.Send(TeamMessages.Invite(myId));
        logger.LogInformation("[{Session}] {Name} invites {Target} to the team", session.Id, session.ActiveCharacter!.Name, target.ActiveCharacter!.Name);
        return ValueTask.CompletedTask;
    }

    /// <summary>Tells whether the other player has switched party invitations off, and if so tells the asker.</summary>
    /// <param name="session">The asker's session.</param>
    /// <param name="other">The other player's session.</param>
    /// <returns>True when the other player refuses.</returns>
    private static bool RefusesParties(GameSession session, GameSession other)
    {
        if (other.ActiveCharacter?.PartyOption != 2)
            return false;
        session.Send(session.Client == ClientGeneration.Client2014
            ? new PacketWriter(Action33.Code, Action33.Option1).U8(Action33.PartyOptionOff).ToArray()
            : TeamMessages.Busy());
        return true;
    }

    /// <summary>Handles the answer to an invitation; on acceptance the answerer joins the inviter.</summary>
    /// <param name="session">The answerer's session.</param>
    /// <param name="body">The packet body: the answer byte, then the inviter's login id.</param>
    internal ValueTask OnInviteReply(GameSession session, ReadOnlyMemory<byte> body)
    {
        if (body.Length < 5 || OtherOf(session, body, 1) is not (var myId, var inviter, var inviterId))
            return ValueTask.CompletedTask;
        var answer = body.Span[0];
        if (session.Team.PendingInvite != inviterId)
            return ValueTask.CompletedTask;
        session.Team.PendingInvite = 0;
        inviter.Send(TeamMessages.InviteAnswer(answer, myId));
        if (answer == 1)
            Join(inviter, inviterId, session, myId);
        return ValueTask.CompletedTask;
    }

    /// <summary>Takes the player out of their team.</summary>
    /// <param name="session">The player's session.</param>
    /// <param name="body">The packet body; not read.</param>
    internal ValueTask OnLeave(GameSession session, ReadOnlyMemory<byte> body)
    {
        if (session.State == SessionState.InWorld && session.Account is not null)
            world.LeaveTeam(session);
        return ValueTask.CompletedTask;
    }

    /// <summary>Takes a member out of the team of the player.</summary>
    /// <param name="session">The player's session.</param>
    /// <param name="body">The packet body: the member's login id.</param>
    internal ValueTask OnKick(GameSession session, ReadOnlyMemory<byte> body)
    {
        if (session.State != SessionState.InWorld || session.Account is null || body.Length < 4)
            return ValueTask.CompletedTask;
        var memberId = BinaryPrimitives.ReadInt32LittleEndian(body.Span);
        var myId = session.Account.LoginId;
        if (!world.Teams.MembersOf(myId).Contains(memberId))
            return ValueTask.CompletedTask;
        if (world.SessionOf(memberId) is { } member)
            world.LeaveTeam(member);
        else
            world.Teams.Remove(memberId);
        return ValueTask.CompletedTask;
    }

    /// <summary>Sends a chat line to every online member of the speaker's team.</summary>
    /// <param name="session">The speaker's session.</param>
    /// <param name="channel">The chat channel.</param>
    /// <param name="text">The chat text.</param>
    public void Chat(GameSession session, byte channel, string text)
    {
        var myId = session.Account!.LoginId;
        var packet = WorldServerMessages.Chat(channel, myId, text);
        foreach (var id in world.Teams.TeamOf(myId))
        {
            if (world.SessionOf(id) is { } s)
                s.Send(packet);
        }
    }

    /// <summary>Adds a member to a leader's team, moves them to the leader's position and tells the map and the team.</summary>
    /// <param name="leader">The leader's session.</param>
    /// <param name="leaderId">The leader's login id.</param>
    /// <param name="member">The new member's session.</param>
    /// <param name="memberId">The new member's login id.</param>
    private void Join(GameSession leader, int leaderId, GameSession member, int memberId)
    {
        if (!world.Teams.Add(leaderId, memberId))
        {
            logger.LogWarning("[{Session}] {Member} cannot join {Leader}'s team (full or taken)", member.Id, memberId, leaderId);
            return;
        }
        var c = member.ActiveCharacter!;
        (c.X, c.Y) = (leader.ActiveCharacter!.X, leader.ActiveCharacter.Y);
        world.SendToMapAndTeam(leader, TeamMessages.Joined(leaderId, memberId), world.Teams.TeamOf(leaderId));
        vitals.Introduce(member);
        logger.LogInformation("[{Session}] {Member} joins {Leader}'s team", member.Id, c.Name, leader.ActiveCharacter.Name);
    }

    /// <summary>Reads another player's login id from the body and finds them on the same map.</summary>
    /// <param name="session">The player's session.</param>
    /// <param name="body">The packet body.</param>
    /// <param name="offset">Where the login id starts in the body.</param>
    /// <returns>The player's own login id, the other session and its login id, or null when the other is not there.</returns>
    private (int MyId, GameSession Other, int OtherId)? OtherOf(GameSession session, ReadOnlyMemory<byte> body, int offset)
    {
        if (session.State != SessionState.InWorld || session.ActiveCharacter is not { } me || session.Account is null || body.Length < offset + 4)
            return null;
        var myId = session.Account.LoginId;
        var otherId = BinaryPrimitives.ReadInt32LittleEndian(body.Span[offset..]);
        if (otherId == myId || world.SessionOf(otherId) is not { } other || other.ActiveCharacter?.MapId != me.MapId)
        {
            logger.LogInformation("[{Session}] team: player {Id} is not here", session.Id, otherId);
            return null;
        }
        return (myId, other, otherId);
    }
}
