namespace NorthIsland.Server.Features.Minigames;

/// <summary>One Lucky Cards play in progress.</summary>
public sealed class PokeRound
{
    /// <summary>The face of each card, indexed 1..8; dealt again after a pair while flips remain.</summary>
    public byte[] Faces { get; set; } = new byte[LuckyCards.Cards + 1];

    /// <summary>The cards flipped so far.</summary>
    public int Flips { get; set; }

    /// <summary>The first card of the pair being flipped.</summary>
    public byte OpenCard { get; set; }

    /// <summary>The pairs found so far.</summary>
    public int Pairs { get; set; }

    /// <summary>The item id the last pair paid; 0 before the first pair.</summary>
    public ushort LastPrize { get; set; }
}
