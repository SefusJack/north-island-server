using NorthIsland.Protocol.Opcodes;

namespace NorthIsland.Server.Actions;

/// <summary>Registers the routes of the packets the client sends with action 85: instances.</summary>
/// <param name="instance">The <c>InstanceHandlers</c> that serve the packets of this action.</param>
public sealed class Action85Instances(
    InstanceHandlers instance) : IAction
{
    public byte Code => Action85.Code;

    public void Register(ActionRoutes routes) => routes
        .On(Action85.OpenList, "instance list", (s, _) => InstanceHandlers.Handle(s, () => instance.OpenList(s))) // 85/1: open the instance window
        .On(Action85.ListPage, "instance list page", instance.OnListPage) // 85/2: ask for a page of the instance list
        .On(Action85.Create, "instance create", (s, b) => InstanceHandlers.Handle(s, () => instance.Create(s, b))) // 85/3: create an instance
        .On(Action85.Detail, "instance detail", (s, b) => InstanceHandlers.Handle(s, () => instance.Detail(s, b))) // 85/4: ask for the details of an instance
        .On(Action85.Join, "instance join", (s, b) => InstanceHandlers.Handle(s, () => instance.Join(s, b))) // 85/5: join an instance
        .On(Action85.Leave, "instance leave", (s, _) => InstanceHandlers.Handle(s, () => instance.Leave(s))) // 85/6: leave the instance
        .On(Action85.Enter, "instance enter", (s, _) => InstanceHandlers.Handle(s, () => instance.Enter(s))) // 85/9: start the instance and move to its entry
        .On(Action85.MembersTab, "instance members", (s, _) => InstanceHandlers.Handle(s, () => instance.SendMembers(s, 1))) // 85/10: show members
        .On(Action85.MemberPage, "instance member page", instance.OnMemberPage) // 85/11: ask for a page of the member list
        .On(Action85.Progress, "instance progress", (s, _) => InstanceHandlers.Handle(s, () => instance.SendProgress(s))) // 85/12: ask for progress
        .On(Action85.Kick, "instance kick", (s, b) => InstanceHandlers.Handle(s, () => instance.Kick(s, b))) // 85/13: creator removes member
        .On(Action85.LeaveGuild, "instance guild leave", (s, _) => InstanceHandlers.Handle(s, () => { })); // 85/14: leave the guild instance, does nothing
}
