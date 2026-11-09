using Microsoft.Extensions.Logging;
using NorthIsland.Protocol.Messages.Items;

namespace NorthIsland.Server.Features.Items;

/// <summary>Dyes in the bag: using one changes the colour of the player's hair, skin, eyes or clothes.</summary>
/// <param name="items">Item definitions, to find what a dye colours.</param>
/// <param name="world">The online players, to show the new colour to others.</param>
/// <param name="logger">Logger.</param>
public sealed class DyeHandlers(
    ItemData items,
    WorldState world,
    ILogger<DyeHandlers> logger)
{

    /// <summary>The part of the look a dye colours, by the item's sub type.</summary>
    /// <param name="subType">The item sub type.</param>
    /// <returns>The part, <see cref="DyePart.None"/> when the item is not a dye.</returns>
    public static DyePart PartOf(byte subType) => subType switch
    {
        56 => DyePart.Hair, // hair dye
        57 => DyePart.Skin, // skin dye
        102 => DyePart.Eyes, // eye dye
        148 => DyePart.Clothes, // clothes dye
        _ => DyePart.None, // not a dye
    };

    /// <summary>Puts a new colour on one part of the two stored colour codes.</summary>
    /// <param name="colour1">The first colour code (hair and skin).</param>
    /// <param name="colour2">The second colour code (clothes and eyes).</param>
    /// <param name="part">The part to colour.</param>
    /// <param name="r">The red level, 0 to 9.</param>
    /// <param name="g">The green level, 0 to 9.</param>
    /// <param name="b">The blue level, 0 to 9.</param>
    /// <returns>The two colour codes after the change; unchanged when <paramref name="part"/> is <see cref="DyePart.None"/>.</returns>
    public static (int Colour1, int Colour2) Recolour(
        int colour1,
        int colour2,
        DyePart part,
        int r,
        int g,
        int b)
    {
        var triple = Math.Clamp(r, 0, 9) * 100 + Math.Clamp(g, 0, 9) * 10 + Math.Clamp(b, 0, 9);
        static int Set(int code, int place, int triple) => code - (code / place % 1000) * place + triple * place;
        return part switch
        {
            DyePart.Hair => (Set(colour1, 1000, triple), colour2), // hair: first colour code
            DyePart.Skin => (Set(colour1, 1, triple), colour2), // skin: first colour code
            DyePart.Clothes => (colour1, Set(colour2, 1_000_000, triple)), // clothes: second colour code
            DyePart.Eyes => (colour1, Set(colour2, 1000, triple)), // eyes: second colour code
            _ => (colour1, colour2), // no part: both codes unchanged
        };
    }

    /// <summary>Handles a dye use: takes one dye from the bag slot, recolours the part and sends the new colour code to the player and everyone.</summary>
    /// <param name="session">The player's session.</param>
    /// <param name="body">The packet body: bag slot, then red, green and blue levels.</param>
    internal ValueTask OnDye(GameSession session, ReadOnlyMemory<byte> body)
    {
        var character = session.ActiveCharacter;
        if (session.State != SessionState.InWorld || character is null || session.Account is null || body.Length < 4)
            return ValueTask.CompletedTask;
        var (slot, r, g, b) = (body.Span[0], body.Span[1], body.Span[2], body.Span[3]);
        var stack = character.Bag.Find(s => s.Slot == slot);
        var part = stack is null ? DyePart.None : PartOf(items[stack.ItemId]?.SubType ?? 0);
        if (stack is null || part == DyePart.None || r > 9 || g > 9 || b > 9)
        {
            logger.LogWarning("[{Session}] dye: bag slot {Slot} holds no dye (or colour {R}{G}{B} out of range)", session.Id, slot, r, g, b);
            return ValueTask.CompletedTask;
        }
        stack.Count--;
        if (stack.Count == 0)
            character.Bag.Remove(stack);
        session.Send(ItemMessages.BagSlotDecrease(slot, 1));

        (character.Colour1, character.Colour2) = Recolour(character.Colour1, character.Colour2, part, r, g, b);
        var second = part is DyePart.Clothes or DyePart.Eyes;
        var packet = ItemMessages.ColourCode(second, session.Account.LoginId, second ? character.Colour2 : character.Colour1);
        session.Send(packet);
        world.Broadcast(session, packet, sameMapOnly: false);
        logger.LogInformation("[{Session}] {Name} dyes the {Part} {R}{G}{B} (colours {C1} / {C2})", session.Id, character.Name, part, r, g, b,
            character.Colour1, character.Colour2);
        return ValueTask.CompletedTask;
    }
}
