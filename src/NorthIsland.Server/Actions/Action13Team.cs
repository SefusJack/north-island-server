using NorthIsland.Protocol.Opcodes;

namespace NorthIsland.Server.Actions;

/// <summary>Registers the routes of the packets the client sends with action 13: teams.</summary>
/// <param name="team">The <c>TeamHandlers</c> that serve the packets of this action.</param>
public sealed class Action13Team(
    TeamHandlers team) : IAction
{
    public byte Code => Action13.Code;

    public void Register(ActionRoutes routes) => routes
        .On(Action13.JoinRequest, "team join request", team.OnJoinRequest) // 13/1: ask to join another player's team
        .On(Action13.JoinAnswer, "team join answer", team.OnJoinAnswer) // 13/3: the leader's answer to a join request
        .On(Action13.Left, "leave team", team.OnLeave) // 13/4: leave the team
        .On(Action13.CounsellorOn, "team counsellor on", (s, b) => team.OnCounsellor(s, b, on: true)) // 13/5: mark a team member as counsellor
        .On(Action13.CounsellorOff, "team counsellor off", (s, b) => team.OnCounsellor(s, b, on: false)) // 13/6: remove the counsellor mark from a team member
        .On(Action13.InviteRequest, "team invite", team.OnInvite) // 13/7: invite a player into the team
        .On(Action13.InviteReply, "team invite answer", team.OnInviteReply) // 13/8: the answer to a team invitation
        .On(Action13.Kick, "team kick", team.OnKick) // 13/9: remove a member from the team
        .On(Action13.LeaderChange, "team leader change", team.OnLeaderChange); // 13/10: hand the team lead to another member
}
