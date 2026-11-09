namespace NorthIsland.Server.Features.Marriage;

/// <summary>A married pair, stored with a snapshot card of each partner.</summary>
public sealed class Couple
{
    /// <summary>The card of the husband (the partner with a male body type).</summary>
    public SpouseCard Husband { get; set; } = new();

    /// <summary>The card of the wife.</summary>
    public SpouseCard Wife { get; set; } = new();

    /// <summary>When the wedding took place, as an OLE automation date (days since 30 December 1899).</summary>
    public double Married { get; set; }

    /// <summary>Tells whether a player is one of the two partners.</summary>
    /// <param name="id">The player's login id.</param>
    /// <returns>True when the id belongs to the husband or the wife.</returns>
    public bool Has(int id) => Husband.Id == id || Wife.Id == id;

    /// <summary>Finds the card of the other partner.</summary>
    /// <param name="id">The player's login id.</param>
    /// <returns>The wife's card when the id is the husband's, otherwise the husband's.</returns>
    public SpouseCard SpouseOf(int id) => Husband.Id == id ? Wife : Husband;

    /// <summary>Finds the card of a partner.</summary>
    /// <param name="id">The player's login id.</param>
    /// <returns>The husband's card when the id is the husband's, otherwise the wife's.</returns>
    public SpouseCard CardOf(int id) => Husband.Id == id ? Husband : Wife;
}
