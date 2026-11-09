using NorthIsland.Protocol.Opcodes;

namespace NorthIsland.Server.Actions;

/// <summary>Registers the routes of the packets the client sends with action 82: the notice boards.</summary>
/// <param name="board">The <c>BoardHandlers</c> that serve the packets of this action.</param>
public sealed class Action82Notices(
    BoardHandlers board) : IAction
{
    public byte Code => Action82.Code;

    public void Register(ActionRoutes routes) => routes
        .On(Action82.Submit, "post notice", (s, b) => board.OnSubmit(s, b, guild: false)) // 82/1: post a notice
        .On(Action82.List, "notice page", (s, b) => board.OnList(s, b, guild: false)) // 82/2: ask for a page of notices
        .On(Action82.Open, "open notice", (s, b) => board.OnOpen(s, b, guild: false)) // 82/3: open a notice
        .On(Action82.Delete, "delete notice", (s, b) => board.OnDelete(s, b, guild: false)) // 82/4: delete a notice
        .On(Action82.AskAdd, "ask to post", (s, _) => board.OnAskAdd(s, guild: false)) // 82/5: ask to write a notice, shows the form or says it is full
        .On(Action82.Reply, "reply to notice", board.OnReply) // 82/6: mail a reply to a notice's author
        .On(Action82.GuildSubmit, "post guild notice", (s, b) => board.OnSubmit(s, b, guild: true)) // 82/7: post a guild notice
        .On(Action82.GuildList, "guild notice page", (s, b) => board.OnList(s, b, guild: true)) // 82/8: ask for a page of guild notices
        .On(Action82.GuildOpen, "open guild notice", (s, b) => board.OnOpen(s, b, guild: true)) // 82/9: open a guild notice
        .On(Action82.GuildDelete, "delete guild notice", (s, b) => board.OnDelete(s, b, guild: true)) // 82/10: delete a guild notice
        .On(Action82.GuildAskAdd, "ask to post in guild", (s, _) => board.OnAskAdd(s, guild: true)); // 82/11: ask to write a guild notice
}
