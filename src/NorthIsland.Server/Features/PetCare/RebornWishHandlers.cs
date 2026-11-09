using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NorthIsland.GameData.Npcs;
using NorthIsland.Protocol.Messages.Items;
using NorthIsland.Protocol.Messages.Player;

namespace NorthIsland.Server.Features.PetCare;

/// <summary>Handles the reborn wish: listing the pets a player may wish for and giving the chosen one for stars.</summary>
/// <param name="npcs">The NPC data, for the pets and their grow keys.</param>
/// <param name="marks">The mark data, to check the career mark flags.</param>
/// <param name="formulas">The formula data, for the new pet's HP and SP.</param>
/// <param name="options">The pet options, which hold the reborn wish settings.</param>
/// <param name="logger">The logger.</param>
public sealed class RebornWishHandlers(
    NpcData npcs,
    MarkData marks,
    FormulaData formulas,
    IOptions<PetOptions> options,
    ILogger<RebornWishHandlers> logger)
{
    /// <summary>Lists the pets a player may wish for: not wished yet, with a grow key from 1 to 50, ordered by grow key.</summary>
    /// <param name="c">The player's character.</param>
    /// <returns>The NPC records.</returns>
    public IReadOnlyList<NpcRecord> Offered(Character c)
    {
        var o = options.Value.RebornWish;
        IEnumerable<NpcRecord> all = o.Pets.Count > 0
            ? o.Pets.Select(id => npcs[id]).OfType<NpcRecord>()
            : npcs.All.Where(n => RebornWish.StarCost.ContainsKey(n.GrowKey));
        return [.. all.Where(n => n.GrowKey is >= 1 and <= 50 && !c.RebornWishes.Contains(n.Id)).OrderBy(n => n.GrowKey)];
    }

    /// <summary>Opens the wish: sends the list of grow keys on offer and the choice screen.</summary>
    /// <param name="session">The player's session.</param>
    public void Open(GameSession session)
    {
        var c = session.ActiveCharacter!;
        var offered = options.Value.RebornWish.Enabled ? Offered(c) : [];
        session.Send(RebornWishMessages.List([.. offered.Select(n => n.GrowKey)]));
        session.Send(RebornWishMessages.Choice());
        logger.LogInformation("[{Session}] {Name}: reborn wish, {Count} pet(s) offered", session.Id, c.Name, offered.Count);
    }

    /// <summary>
    /// Grants a wish: takes the stars, gives the pet in a free slot and removes it from the list; refused when the pet is
    /// not on offer, costs nothing, or the player lacks a slot or stars.
    /// </summary>
    /// <param name="session">The player's session.</param>
    /// <param name="body">The packet body: the pet's NPC id.</param>
    internal ValueTask OnWish(GameSession session, ReadOnlyMemory<byte> body)
    {
        if (session.State != SessionState.InWorld || session.ActiveCharacter is not { } c || body.Length < 2)
            return ValueTask.CompletedTask;
        var o = options.Value.RebornWish;
        var npcId = (ushort)(body.Span[0] | body.Span[1] << 8);
        var pet = o.Enabled ? Offered(c).FirstOrDefault(n => n.Id == npcId) : null;
        if (pet is null)
        {
            logger.LogWarning("[{Session}] {Name}: reborn wish for {Npc}, which is not on the list", session.Id, c.Name, npcId);
            return ValueTask.CompletedTask;
        }
        var cost = RebornWish.Cost(pet.GrowKey, pet.Id, mark => marks.FlagBit(mark) != 0 && c.MarkFlags.Contains(mark));
        if (cost == 0 || Pets.FreeSlot(c) == 0 || c.Pets.Exists(p => p.NpcId == pet.Id) || Bag.Count(c, o.StarItem) < cost)
        {
            logger.LogInformation("[{Session}] {Name}: reborn wish for {Pet} refused ({Cost} stars, {Have} in the bag, {Pets} pets)", session.Id, c.Name,
                pet.Name, cost, Bag.Count(c, o.StarItem), c.Pets.Count);
            return ValueTask.CompletedTask;
        }

        Bag.Remove(c, o.StarItem, cost);
        session.Send(ItemMessages.BagRemoveById(o.StarItem, (byte)cost));
        var given = Pets.Create(pet, Pets.FreeSlot(c), formulas.Table);
        c.Pets.Add(given);
        session.Send(PetMessages.Joined(session.Account!.LoginId, Pets.Info(given)));
        c.RebornWishes.Add(pet.Id);
        session.Send(RebornWishMessages.Removed(pet.Id));
        logger.LogInformation(
            "[{Session}] {Name}: reborn wish, {Pet} ({Npc}) for {Cost} stars, slot {Slot}",
            session.Id,
            c.Name,
            pet.Name,
            pet.Id,
            cost,
            given.Slot);
        return ValueTask.CompletedTask;
    }
}
