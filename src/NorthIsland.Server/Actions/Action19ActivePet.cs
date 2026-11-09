using NorthIsland.Protocol.Opcodes;

namespace NorthIsland.Server.Actions;

/// <summary>Registers the routes of the packets the client sends with action 19: the active pet joining combat or resting.</summary>
/// <param name="pet">The <c>PetHandlers</c> that serve the packets of this action.</param>
public sealed class Action19ActivePet(
    PetHandlers pet) : IAction
{
    public byte Code => Action19.Code;

    public void Register(ActionRoutes routes) => routes
        .On(Action19.JoinCombat, "pet joins combat", pet.OnJoin) // 19/1: the active pet joins the player in combat
        .On(Action19.Rest, "pet rests", pet.OnRest); // 19/2: the active pet rests and is hidden from the map
}
