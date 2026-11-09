namespace NorthIsland.Server.Features.Trading;

/// <summary>One side of a trade: who it is and what they offer.</summary>
/// <param name="session">The session of this side.</param>
public sealed class TradeSide(
    GameSession session)
{
    /// <summary>The session of this side.</summary>
    public GameSession Session { get; } = session;

    /// <summary>The gold offered.</summary>
    public int Gold { get; set; }

    /// <summary>The bag slots offered.</summary>
    public List<byte> BagSlots { get; } = [];

    /// <summary>The slot of the pet offered, 0 for none.</summary>
    public byte PetSlot { get; set; }

    /// <summary>Whether an offer has been made.</summary>
    public bool Offered { get; set; }

    /// <summary>Whether this side has confirmed the current offer; a new offer clears it.</summary>
    public bool Confirmed { get; set; }
}
