using NorthIsland.Protocol.Opcodes;

namespace NorthIsland.Server.Actions;

/// <summary>Registers the routes of the packets the client sends with action 39: guilds.</summary>
/// <param name="guildEconomy">The <c>GuildEconomyHandlers</c> that serve the packets of this action.</param>
/// <param name="guild">The <c>GuildHandlers</c> that serve the packets of this action.</param>
public sealed class Action39Guild(
    GuildEconomyHandlers guildEconomy,
    GuildHandlers guild) : IAction
{
    public byte Code => Action39.Code;

    public void Register(ActionRoutes routes) => routes
        .On(Action39.Create, "create guild", guild.OnCreate) // 39/1: create a guild
        .On(Action39.Invite, "guild invite", guild.OnInvite) // 39/2: invite a player into the guild
        .On(Action39.Answer, "guild invite answer", guild.OnAnswer) // 39/3: the answer to a guild invitation
        .On(Action39.Mail, "guild mail", guildEconomy.OnMail) // 39/4: send a letter to guild members
        .On(Action39.Leave, "leave guild", guild.OnLeave) // 39/6: leave the guild
        .On(Action39.Kick, "kick guild member", guild.OnKick) // 39/7: remove a member from the guild
        .On(Action39.Disband, "disband guild", guild.OnDisband) // 39/8: disband the guild
        .On(Action39.Rules, "guild rules", guild.OnRules) // 39/9: set the guild rules
        .On(Action39.AppointVice, "offer vice post", (s, b) => guild.OnOffer(s, b, lord: false)) // 39/10: offer the vice post to a member
        .On(Action39.DeposeVice, "depose vice", guild.OnDepose) // 39/11: take the vice post from a member
        .On(Action39.Abdicate, "offer lord post", (s, b) => guild.OnOffer(s, b, lord: true)) // 39/12: offer the lord post to a member
        .On(Action39.LordAnswer, "lord answer", (s, b) => guild.OnPostAnswer(s, b, lord: true)) // 39/13: the answer to an offer of the lord post
        .On(Action39.ViceAnswer, "vice answer", (s, b) => guild.OnPostAnswer(s, b, lord: false)) // 39/14: the answer to an offer of the vice post
        .On(Action39.Rights, "guild rights", guildEconomy.OnRights) // 39/16: guild rights
        .On(Action39.UploadBadge, "upload guild badge", guild.OnUploadBadge) // 39/18: upload the guild badge
        .On(Action39.BadgeRequest, "request guild badges", guild.OnBadgeRequest); // 39/19: ask for the badge images of guilds
}
