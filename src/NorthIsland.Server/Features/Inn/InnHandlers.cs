using Microsoft.Extensions.Logging;
using NorthIsland.Protocol.Messages.Player;
using NorthIsland.Protocol.Opcodes;

namespace NorthIsland.Server.Features.Inn;

/// <summary>Handles the inn rest: offering a rest for a price, and healing the team and its pets when the player accepts.</summary>
/// <param name="world">The world state, used to find the team members and their sessions.</param>
/// <param name="maps">The map data, used to find the npc of the running event.</param>
/// <param name="logger">The logger.</param>
public sealed class InnHandlers(
    WorldState world,
    MapData maps,
    ILogger<InnHandlers> logger)
{
    /// <summary>
    /// Offers the player a rest at the inn and remembers the price. The price is sent with <c>31/2</c>; <c>-1</c> tells the client that nobody
    /// needs to rest. The rest is free when the event runs at the Curer npc.
    /// </summary>
    /// <param name="session">The player's session.</param>
    /// <param name="character">The player's active character.</param>
    public void OfferRest(GameSession session, Character character)
    {
        if (Resting(session).All(r => !NeedsRest(r.Character)))
        {
            session.Inn.PendingPrice = null;
            session.Send(InnMessages.RestPrice(-1));
            logger.LogInformation("[{Session}] inn: {Name} does not need to rest", session.Id, character.Name);
            return;
        }
        var price = IsFreeCurer(session, character) ? 0 : RestPrice(character.Level);
        session.Inn.PendingPrice = price;
        session.Send(InnMessages.RestPrice(price));
        logger.LogInformation("[{Session}] inn: rest offered to {Name} for {Price} gold", session.Id, character.Name, price);
    }

    /// <summary>Returns the price of a rest by character level: free up to level 10, then a rising multiple of the level.</summary>
    /// <param name="level">The character's level.</param>
    /// <returns>The price in gold.</returns>
    public static int RestPrice(int level) => level switch
    {
        <= 10 => 0, // free up to level 10
        <= 30 => level * 2, // 2 gold per level up to level 30
        <= 60 => level * 3, // 3 gold per level up to level 60
        <= 100 => level * 4, // 4 gold per level up to level 100
        <= 150 => level * 5, // 5 gold per level up to level 150
        _ => level * 6, // 6 gold per level above 150
    };

    /// <summary>The npc id of the Curer, whose rest is free.</summary>
    public const int Curer = 14176; // Curer

    /// <summary>Returns whether the event the player runs was started at the Curer npc on their map.</summary>
    /// <param name="session">The player's session.</param>
    /// <param name="character">The player's active character.</param>
    /// <returns>True when the rest is free.</returns>
    private bool IsFreeCurer(GameSession session, Character character) =>
        session.Event is { Trigger: >= EventEngine.TriggerClick and <= EventEngine.TriggerArea } run && run.MapId == character.MapId
            && maps.Scene(character.MapId)?.NpcByRoleIndex(run.Index) is { NpcId: Curer };

    /// <summary>
    /// Handles accepting the offered rest (<c>31/1</c>): takes the price, answers the client, and restores HP and SP of everyone who rests with the
    /// player and of their pets. It is refused without an offer, and answered with a failure when the gold is not enough.
    /// </summary>
    /// <param name="session">The player's session.</param>
    /// <param name="body">The packet body after the action and sub code.</param>
    internal ValueTask OnAccept(GameSession session, ReadOnlyMemory<byte> body)
    {
        var character = session.ActiveCharacter;
        if (session.State != SessionState.InWorld || character is null || session.Inn.PendingPrice is not { } price)
        {
            logger.LogWarning("[{Session}] inn rest accepted without an offer", session.Id);
            return ValueTask.CompletedTask;
        }
        session.Inn.PendingPrice = null;
        if (character.Gold < price)
        {
            session.Send(InnMessages.RestAnswer(1));
            return ValueTask.CompletedTask;
        }

        character.Gold -= price;
        if (price > 0)
            session.Send(PlayerMessages.MoneyChange(-price));
        session.Send(InnMessages.RestAnswer(0));
        foreach (var (guest, guestCharacter) in Resting(session))
        {
            guestCharacter.Hp = guestCharacter.MaxHp;
            guestCharacter.Sp = guestCharacter.MaxSp;
            guest.Send(InnMessages.Sleep());
            guest.Send(InnMessages.SetAttribute(Action8.AttrHp, guestCharacter.Hp));
            guest.Send(InnMessages.SetAttribute(Action8.AttrSp, guestCharacter.Sp));
            foreach (var pet in guestCharacter.Pets)
            {
                pet.Hp = pet.MaxHp;
                pet.Sp = pet.MaxSp;
                guest.Send(PetMessages.SetAttribute(pet.Slot, Action8.AttrHp, pet.Hp));
                guest.Send(PetMessages.SetAttribute(pet.Slot, Action8.AttrSp, pet.Sp));
            }
        }
        logger.LogInformation("[{Session}] inn: {Name} rests for {Price} gold (HP {Hp}, SP {Sp}), {Count} rested in all", session.Id, character.Name, price,
            character.Hp, character.Sp, Resting(session).Count);
        return ValueTask.CompletedTask;
    }

    /// <summary>Returns whether the character or any of their pets is below full HP or SP.</summary>
    /// <param name="c">The character.</param>
    /// <returns>True when a rest would change something.</returns>
    private static bool NeedsRest(Character c) =>
        c.Hp < c.MaxHp || c.Sp < c.MaxSp || c.Pets.Any(p => p.Hp < p.MaxHp || p.Sp < p.MaxSp);

    /// <summary>Lists the players who rest together: the player and the members of their team who are online on the same map.</summary>
    /// <param name="session">The session of the player who rests.</param>
    /// <returns>The sessions and active characters of everyone who rests.</returns>
    private List<(GameSession Session, Character Character)> Resting(GameSession session)
    {
        var self = session.ActiveCharacter!;
        List<(GameSession, Character)> guests = [(session, self)];
        foreach (var id in world.Teams.TeamOf(session.Account!.LoginId))
            if (id != session.Account.LoginId && world.SessionOf(id) is { } other && other.ActiveCharacter is { } c && c.MapId == self.MapId)
                guests.Add((other, c));
        return guests;
    }
}
