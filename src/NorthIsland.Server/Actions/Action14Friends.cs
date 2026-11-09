using NorthIsland.Protocol.Opcodes;

namespace NorthIsland.Server.Actions;

/// <summary>Registers the routes of the packets the client sends with action 14: friends, mail, the blocklist and furniture mail.</summary>
/// <param name="furMail">The <c>FurMailHandlers</c> that serve the packets of this action.</param>
/// <param name="blocklist">The <c>BlocklistHandlers</c> that serve the packets of this action.</param>
/// <param name="friend">The <c>FriendHandlers</c> that serve the packets of this action.</param>
public sealed class Action14Friends(
    FurMailHandlers furMail,
    BlocklistHandlers blocklist,
    FriendHandlers friend) : IAction
{
    public byte Code => Action14.Code;

    public void Register(ActionRoutes routes) => routes
        .On(Action14.Mail, "send mail", friend.OnMail) // 14/1: send a mail to players
        .On(Action14.FriendRequest, "friend request", friend.OnRequest) // 14/2: ask another player to be friends
        .On(Action14.FriendAnswer, "friend answer", friend.OnAnswer) // 14/3: answer a friend request
        .On(Action14.FriendRemoved, "delete friend", friend.OnDelete) // 14/4: delete a friend
        .On(Action14.Blocklist, "blocklist", blocklist.OnBlocklist) // 14/5: add to, remove from or ask for the block list
        .On(Action14.FurMail, "furniture mail", furMail.OnLetter); // 14/6: send a letter from a piece of furniture
}
