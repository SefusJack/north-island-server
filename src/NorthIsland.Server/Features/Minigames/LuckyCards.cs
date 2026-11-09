namespace NorthIsland.Server.Features.Minigames;

/// <summary>The Lucky Cards game: the sport kind, the card layout and the draws.</summary>
public static class LuckyCards
{
    /// <summary>Sport kind of Lucky Cards.</summary>
    public const byte Kind = 16;

    /// <summary>The cards on the table (8), the picture faces to choose from (15) and the card flips one play allows (4).</summary>
    public const int Cards = 8, Faces = 15, FlipsPerPlay = 4;

    /// <summary>The item id given for a play without a pair (the Lottery Ticket).</summary>
    public const ushort Consolation = 30063; // Lottery Ticket

    /// <summary>Deals the cards: four different faces, each on two cards, in random order.</summary>
    /// <param name="roll">Random source: <c>roll(n)</c> returns an integer from 0 to n - 1.</param>
    /// <returns>The face of each card, indexed 1..8; entry 0 is not used.</returns>
    public static byte[] Deal(Func<int, int> roll)
    {
        List<byte> pictures = [];
        while (pictures.Count < Cards / 2)
        {
            var face = (byte)(roll(Faces) + 1);
            if (!pictures.Contains(face))
                pictures.Add(face);
        }
        List<byte> deck = [.. pictures, .. pictures];
        var faces = new byte[Cards + 1];
        for (var card = 1; card <= Cards; card++)
        {
            var pick = roll(deck.Count);
            faces[card] = deck[pick];
            deck.RemoveAt(pick);
        }
        return faces;
    }

    /// <summary>Draws the prize for a matched pair, one of the Fortune Egg prizes.</summary>
    /// <param name="roll">Random source: <c>roll(n)</c> returns an integer from 0 to n - 1.</param>
    /// <returns>The prize item id.</returns>
    public static ushort PairPrize(Func<int, int> roll) => Carnival.FortuneEggPrizes[roll(Carnival.FortuneEggPrizes.Length)];
}
