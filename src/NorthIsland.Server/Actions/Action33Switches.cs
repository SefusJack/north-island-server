using NorthIsland.Protocol.Opcodes;

namespace NorthIsland.Server.Actions;

/// <summary>Registers the routes of the packets the client sends with action 33: system options.</summary>
/// <param name="social">The <c>SocialHandlers</c> that serve the packets of this action.</param>
public sealed class Action33Switches(
    SocialHandlers social) : IAction
{
    public byte Code => Action33.Code;

    public void Register(ActionRoutes routes) => routes
        .On(Action33.Option1, "option 1", social.OnOption1) // 33/1: set option 1
        .On(Action33.Option2, "option 2", social.OnOption2) // 33/2: set option 2
        .On(Action33.ChannelBits, "chat channels", social.OnChannelBits) // 33/3: set the chat channel bits
        .On(Action33.Option6, "option 6", social.OnOption6) // 33/4: set option 6
        .On(Action33.PartyOption, "party option", social.OnPartyOption); // 33/5: set the party option
}
