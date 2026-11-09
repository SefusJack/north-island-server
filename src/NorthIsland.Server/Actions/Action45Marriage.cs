using NorthIsland.Protocol.Opcodes;

namespace NorthIsland.Server.Actions;

/// <summary>Registers the routes of the packets the client sends with action 45: marriage.</summary>
/// <param name="marriage">The <c>MarriageHandlers</c> that serve the packets of this action.</param>
public sealed class Action45Marriage(
    MarriageHandlers marriage) : IAction
{
    public byte Code => Action45.Code;

    public void Register(ActionRoutes routes) => routes
        .On(Action45.Refresh, "spouse card", marriage.OnRefresh) // 45/4: resend the spouse card
        .On(Action45.SpouseMail, "spouse mail", marriage.OnSpouseMail) // 45/6: send a mail to the spouse
        .On(Action45.PetFlagClear, "pet flag clear", marriage.OnPetFlagClear) // 45/8: the pets are no longer hidden, show them again
        .On(Action45.ApplicationAnswer, "marriage application answer", marriage.OnApplicationAnswer) // 45/11: the answer to a marriage application
        .On(Action45.DivorceFormAnswer, "divorce answer", marriage.OnDivorceAnswer); // 45/12: the answer to a divorce
}
