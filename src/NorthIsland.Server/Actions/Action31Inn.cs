using NorthIsland.Protocol.Messages.Events;
using NorthIsland.Protocol.Messages.Player;
using NorthIsland.Protocol.Opcodes;

namespace NorthIsland.Server.Actions;

/// <summary>Registers the routes of the packets the client sends with action 31: inn rest and inn pet storage.</summary>
/// <param name="inn">The <c>InnHandlers</c> that serve the packets of this action.</param>
/// <param name="petStorage">The <c>PetStorageHandlers</c> that serve the packets of this action.</param>
public sealed class Action31Inn(
    InnHandlers inn,
    PetStorageHandlers petStorage) : IAction
{
    public byte Code => Action31.Code;

    public void Register(ActionRoutes routes) => routes
        .On(Action31.AcceptRest, "inn rest", inn.OnAccept) // 31/1: rest at the inn
        .On(Action31.Withdraw, "withdraw stored pets", (s, b) => petStorage.OnMove(s, [], b.Span.ToArray())) // 31/2: take stored pets out of the inn
        .On(Action31.Store, "store pets", (s, b) => petStorage.OnMove(s, b.Span.ToArray(), [])) // 31/3: put pets into inn storage
        .On(Action31.Swap, "swap stored pets", petStorage.OnSwap) // 31/4: swap stored pets
        .Clears(Action31.AcceptRest, () => [EventMessages.Finished()], whenSilent: false) // 31/1: sends the event-finished packet if no reply
        .Clears([Action31.Withdraw, Action31.Store, Action31.Swap], () => [InnMessages.StorageDone()], whenSilent: true); // 31/2,3,4: storage done if no reply
}
