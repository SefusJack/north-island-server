using NorthIsland.Protocol.Opcodes;

namespace NorthIsland.Server.Actions;

/// <summary>Registers the routes of the packets the client sends with action 28: learning skills.</summary>
/// <param name="pet">The <c>PetHandlers</c> that serve the packets of this action.</param>
public sealed class Action28Skills(
    PetHandlers pet) : IAction
{
    public byte Code => Action28.Code;

    public void Register(ActionRoutes routes) => routes
        .On(Action28.Learn, "learn skill", pet.OnLearn); // 28/1: a pet learns a skill
}
