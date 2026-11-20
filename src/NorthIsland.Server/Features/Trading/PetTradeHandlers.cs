using System.Buffers.Binary;
using Microsoft.Extensions.Logging;
using NorthIsland.Protocol.Messages.Items;
using NorthIsland.Protocol.Messages.Player;

namespace NorthIsland.Server.Features.Trading;

/// <summary>Serves the pet trade: two players on the same map each offer gold and at most one pet, and both must confirm.</summary>
/// <param name="world">The online sessions.</param>
/// <param name="combat">The combat stats, resent when a player gets off a pet they give away.</param>
/// <param name="logger">The logger.</param>
public sealed class PetTradeHandlers(
    WorldState world,
    CombatStats combat,
    ILogger<PetTradeHandlers> logger)
{
    /// <summary>Cancels the pet trade the session is in, if any, and tells the partner it was cancelled.</summary>
    /// <param name="session">The player's session.</param>
    public static void Abort(GameSession session)
    {
        if (session.Trades.Pets is not { } trade)
            return;
        lock (trade.Lock)
        {
            if (trade.Closed)
                return;
            Close(trade);
        }
        trade.PartnerOf(session).Session.Send(PetTradeMessages.Result(PetTradeResult.Cancelled));
    }

    /// <summary>
    /// Opens a pet trade with another player (<c>25/10</c>), or answers with the reason it is refused: busy with a trade or an event, offline,
    /// other map, in a battle.
    /// </summary>
    /// <param name="session">The player's session.</param>
    /// <param name="body">The packet body: i32 login id of the other player.</param>
    internal ValueTask OnRequest(GameSession session, ReadOnlyMemory<byte> body)
    {
        if (InWorld(session) is not { } character || body.Length < 4)
            return ValueTask.CompletedTask;
        var targetId = BinaryPrimitives.ReadInt32LittleEndian(body.Span);
        PetTradeResult? refusal;
        if (session.Trades.Pets is not null || session.Trades.Items is not null || session.Event is not null)
            refusal = PetTradeResult.Busy;
        else if (world.SessionOf(targetId) is not { } target || target.ActiveCharacter is null || ReferenceEquals(target, session))
            refusal = PetTradeResult.TargetOffline;
        else if (target.ActiveCharacter.MapId != character.MapId)
            refusal = PetTradeResult.DifferentScene;
        else if (target.Trades.Pets is not null || target.Trades.Items is not null || target.Event is not null)
            refusal = PetTradeResult.TargetBusy;
        else if (session.Battle is not null || target.Battle is not null)
            refusal = PetTradeResult.InBattle;
        else
        {
            var trade = new Trade(session, target);
            session.Trades.Pets = trade;
            target.Trades.Pets = trade;
            session.Send(PetTradeMessages.Start(target.Account!.LoginId));
            target.Send(PetTradeMessages.Start(session.Account!.LoginId));
            logger.LogInformation("[{Session}] pet trade opened: {A} with {B}", session.Id, character.Name, target.ActiveCharacter.Name);
            return ValueTask.CompletedTask;
        }
        session.Send(PetTradeMessages.Result(refusal.Value));
        logger.LogInformation("[{Session}] pet trade request to {Target} refused: {Reason}", session.Id, targetId, refusal);
        return ValueTask.CompletedTask;
    }

    /// <summary>
    /// Records the player's offer (<c>25/11</c>) and shows it to the partner. The offer keeps the pet itself, and any offer clears the
    /// confirmation of both sides, so each confirms what the other offers now. An offer of too much gold or an unusable pet cancels the trade.
    /// </summary>
    /// <param name="session">The player's session.</param>
    /// <param name="body">The packet body: i32 gold, optionally u8 pet slot (0 for no pet).</param>
    internal ValueTask OnOffer(GameSession session, ReadOnlyMemory<byte> body)
    {
        if (InWorld(session) is not { } character || session.Trades.Pets is not { } trade || body.Length < 4)
            return ValueTask.CompletedTask;
        var gold = BinaryPrimitives.ReadInt32LittleEndian(body.Span);
        var petSlot = body.Length > 4 ? body.Span[4] : (byte)0;
        var pet = petSlot == 0 ? null : character.Pets.Find(p => p.Slot == petSlot);
        var refusal = gold < 0 || gold > character.Gold ? (PetTradeResult.NoFunds, "not that much gold")
            : petSlot != 0 && pet is null ? (PetTradeResult.Cancelled, "no pet in that slot")
                : pet is null ? null
                    : Untradable(session, character, pet);
        if (refusal is var (r, why))
        {
            logger.LogWarning("[{Session}] pet trade offer refused, {Why}: gold {Gold} (has {Has}), pet slot {Slot}", session.Id, why, gold,
                character.Gold, petSlot);
            Abort(session);
            session.Send(PetTradeMessages.Result(r));
            return ValueTask.CompletedTask;
        }

        GameSession partner;
        lock (trade.Lock)
        {
            if (trade.Closed)
                return ValueTask.CompletedTask;
            var side = trade.Of(session);
            side.Gold = gold;
            side.Pet = pet;
            side.Offered = true;
            trade.First.Confirmed = false;
            trade.Second.Confirmed = false;
            partner = trade.PartnerOf(session).Session;
        }
        partner.Send(PetTradeMessages.PartnerOffer(gold, pet is null ? null : Pets.Info(pet), pet?.MaxHp ?? 0, pet?.MaxSp ?? 0, partner.Client));
        logger.LogInformation("[{Session}] {Name} offers {Gold} gold and {Pet}", session.Id, character.Name, gold, pet?.Name ?? "no pet");
        return ValueTask.CompletedTask;
    }

    /// <summary>
    /// Confirms or cancels the trade (<c>25/12</c>). When both sides have confirmed the trade is carried out and both are told the result.
    /// </summary>
    /// <param name="session">The player's session.</param>
    /// <param name="body">The packet body: u8, 1 to confirm, anything else to cancel.</param>
    internal ValueTask OnDecide(GameSession session, ReadOnlyMemory<byte> body)
    {
        if (InWorld(session) is null || session.Trades.Pets is not { } trade || body.Length < 1)
            return ValueTask.CompletedTask;
        if (body.Span[0] != 1)
        {
            Abort(session);
            session.Send(PetTradeMessages.Result(PetTradeResult.Cancelled));
            return ValueTask.CompletedTask;
        }
        PetTradeResult? result = null;
        lock (trade.Lock)
        {
            if (trade.Closed)
                return ValueTask.CompletedTask;
            var side = trade.Of(session);
            if (!side.Offered)
                return ValueTask.CompletedTask;
            side.Confirmed = true;
            if (trade.First.Confirmed && trade.Second.Confirmed)
            {
                result = Execute(trade);
                Close(trade);
            }
        }
        if (result is { } r)
        {
            trade.First.Session.Send(PetTradeMessages.Result(r));
            trade.Second.Session.Send(PetTradeMessages.Result(r));
        }
        return ValueTask.CompletedTask;
    }

    /// <summary>
    /// Carries out a confirmed trade: checks the gold is still there and fits under the purse limit, that each offered pet is the same pet, still
    /// in the team and still tradable, and that the pets fit, then swaps the pets and moves the gold difference.
    /// </summary>
    /// <param name="trade">The trade, locked by the caller.</param>
    /// <returns>The result sent to both players.</returns>
    private PetTradeResult Execute(Trade trade)
    {
        var (a, b) = (trade.First, trade.Second);
        var (ca, cb) = (a.Session.ActiveCharacter!, b.Session.ActiveCharacter!);
        var (petA, petB) = (a.Pet, b.Pet);
        if (a.Gold > ca.Gold || b.Gold > cb.Gold
            || (long)ca.Gold - a.Gold + b.Gold > BankHandlers.MaxGold || (long)cb.Gold - b.Gold + a.Gold > BankHandlers.MaxGold)
            return PetTradeResult.NoFunds;
        foreach (var (side, c, pet) in new[] { (a, ca, petA), (b, cb, petB) })
        {
            if (pet is not null && Untradable(side.Session, c, pet) is var (r, why))
            {
                logger.LogWarning("[{Session}] {Name}: pet trade cancelled, {Pet}: {Why}", side.Session.Id, c.Name, pet.Name, why);
                return r;
            }
        }
        if (!Fits(ca, petA, petB) || !Fits(cb, petB, petA))
            return ca.Pets.Count - (petA is null ? 0 : 1) + (petB is null ? 0 : 1) > Pet.MaxPets
                || cb.Pets.Count - (petB is null ? 0 : 1) + (petA is null ? 0 : 1) > Pet.MaxPets
                ? PetTradeResult.TooManyPets
                : PetTradeResult.SamePet;

        Give(a.Session, ca, petA);
        Give(b.Session, cb, petB);
        Receive(a.Session, ca, petB);
        Receive(b.Session, cb, petA);
        if (b.Gold - a.Gold is var goldA and not 0)
        {
            ca.Gold += goldA;
            a.Session.Send(PlayerMessages.MoneyChange(goldA));
            cb.Gold -= goldA;
            b.Session.Send(PlayerMessages.MoneyChange(-goldA));
        }
        logger.LogInformation("Pet trade done: {A} gave {PetA} + {GoldA} gold; {B} gave {PetB} + {GoldB} gold", ca.Name, petA?.Name ?? "no pet",
            a.Gold, cb.Name, petB?.Name ?? "no pet", b.Gold);
        return PetTradeResult.Success;
    }

    /// <summary>
    /// Tells whether a character can end up with the received pet after giving theirs: under the pet limit and no pet of the same kind.
    /// </summary>
    /// <param name="c">The character.</param>
    /// <param name="give">The pet they give away, if any.</param>
    /// <param name="receive">The pet they receive, if any.</param>
    /// <returns>True when it fits.</returns>
    private static bool Fits(Character c, Pet? give, Pet? receive) =>
        receive is null
            || c.Pets.Count - (give is null ? 0 : 1) + 1 <= Pet.MaxPets && !c.Pets.Exists(p => p != give && p.NpcId == receive.NpcId);

    /// <summary>
    /// Tells why a pet cannot be offered in a trade: it is no longer in the team, it cannot be traded, it has no amity, it is ridden or it is in the
    /// owner's pet stall.
    /// </summary>
    /// <param name="session">The owner's session.</param>
    /// <param name="c">The owner's character.</param>
    /// <param name="pet">The pet.</param>
    /// <returns>The result to send and the reason for the log, or null when the pet may be offered.</returns>
    private static (PetTradeResult Result, string Why)? Untradable(GameSession session, Character c, Pet pet) =>
        !c.Pets.Exists(p => ReferenceEquals(p, pet)) ? (PetTradeResult.Cancelled, "the pet is no longer in the team")
            : pet.Tradable == 0 ? (PetTradeResult.Cancelled, "the pet cannot be traded")
                : pet.Amity == 0 ? (PetTradeResult.NoAmity, "the pet has no amity")
                    : session.RidingPetSlot == pet.Slot ? (PetTradeResult.Cancelled, "the pet is ridden")
                        : session.Stalls.Pets?.Prices.ContainsKey(pet.Slot) == true ? (PetTradeResult.Cancelled, "the pet is in the pet stall")
                            : null;

    /// <summary>Takes a pet away from a character of this server.</summary>
    /// <param name="session">The player's session.</param>
    /// <param name="c">The character.</param>
    /// <param name="pet">The pet to remove, or null for none.</param>
    private void Give(GameSession session, Character c, Pet? pet) => Give(world, session, c, pet, combat);

    /// <summary>
    /// Takes a pet away from a character: rests and hides it when it is the active pet, gets the player off it when it is ridden (as
    /// <see cref="Pets.Exile"/> does), removes it from the list and tells the client.
    /// </summary>
    /// <param name="world">The online sessions.</param>
    /// <param name="session">The player's session.</param>
    /// <param name="c">The character.</param>
    /// <param name="pet">The pet to remove, or null for none.</param>
    /// <param name="combat">The combat stats, resent when the player gets off the pet; null skips the resend.</param>
    internal static void Give(WorldState world, GameSession session, Character c, Pet? pet, CombatStats? combat = null)
    {
        if (pet is null)
            return;
        if (c.ActivePetId == pet.NpcId)
        {
            c.ActivePetId = 0;
            session.Send(PetMessages.Rest());
            world.HidePet(session);
        }
        if (session.RidingPetSlot == pet.Slot)
        {
            session.RidingPetSlot = 0;
            var off = PetMessages.Dismounted(session.Account!.LoginId);
            session.Send(off);
            world.Broadcast(session, off, sameMapOnly: true);
            combat?.Send(session);
        }
        c.Pets.Remove(pet);
        session.Send(PetMessages.Removed(session.Account!.LoginId, pet.Slot));
    }

    /// <summary>Gives a pet to a character: puts it in a free slot and tells the client.</summary>
    /// <param name="session">The player's session.</param>
    /// <param name="c">The character.</param>
    /// <param name="pet">The pet to add, or null for none.</param>
    internal static void Receive(GameSession session, Character c, Pet? pet)
    {
        if (pet is null)
            return;
        pet.Slot = Pets.FreeSlot(c);
        c.Pets.Add(pet);
        session.Send(PetMessages.Joined(session.Account!.LoginId, Pets.Info(pet)));
    }

    /// <summary>Marks the trade closed and removes it from both sessions.</summary>
    /// <param name="trade">The trade.</param>
    private static void Close(Trade trade)
    {
        trade.Closed = true;
        trade.First.Session.Trades.Pets = null;
        trade.Second.Session.Trades.Pets = null;
    }

    /// <summary>Returns the active character when the session is in the world with an account.</summary>
    /// <param name="session">The player's session.</param>
    /// <returns>The active character, or null when the request must be ignored.</returns>
    private static Character? InWorld(GameSession session)
    {
        var character = session.ActiveCharacter;
        return session.State == SessionState.InWorld && character is not null && session.Account is not null ? character : null;
    }
}
