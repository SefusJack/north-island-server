using NorthIsland.Protocol.Messages.World;
using NorthIsland.Protocol.Opcodes;

namespace NorthIsland.Server.Actions;

/// <summary>Registers the routes of the packets the client sends with action 64: manufacture with a pet.</summary>
/// <param name="manufacture">The <c>ManufactureHandlers</c> that serve the packets of this action.</param>
public sealed class Action64Manufacture(
    ManufactureHandlers manufacture) : IAction
{
    public byte Code => Action64.Code;

    public void Register(ActionRoutes routes) => routes
        .On(Action64.Make, "manufacture", manufacture.OnMake) // 64/1: start making an item
        .On(Action64.Start, "continue manufacture", manufacture.OnContinue) // 64/2: continue a paused manufacture job
        .On(Action64.Stop, "stop manufacture", ManufactureHandlers.OnStop) // 64/3: pause the manufacture job in a slot
        .On(Action64.GiveUp, "give up manufacture", manufacture.OnGiveUp) // 64/4: give up a manufacture job
        .Clears(Action64.Make, () => [HouseMessages.PetRefused()], whenSilent: true) // 64/1: sends back the pet refusal of the pet maker
        .Clears(Action64.Start, () => [HouseMessages.PetRefused()], whenSilent: true); // 64/2: sends back the pet refusal of the pet maker
}
