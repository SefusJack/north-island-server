namespace NorthIsland.Server.Data;

/// <summary>A piece of furniture a character owns in the tent or house, placed in a room or kept (see <see cref="Retained"/>).</summary>
public sealed class Furniture
{
    /// <summary>Most pieces a character can own.</summary>
    public const int MaxPieces = 130;

    /// <summary>Most tools that can sit on one work platform.</summary>
    public const int MaxToolsPerPlatform = 10;

    /// <summary>The piece's id within the character's furniture, 1-based and unique per character; other pieces refer to it.</summary>
    public ushort Index { get; set; }

    /// <summary>Item id of the piece.</summary>
    public ushort ItemId { get; set; }

    /// <summary>X position in the room.</summary>
    public int X { get; set; }

    /// <summary>Y position in the room.</summary>
    public int Y { get; set; }

    /// <summary>Height layer of the piece, 1 on the floor.</summary>
    public int Z { get; set; } = 1;

    /// <summary>Facing direction.</summary>
    public byte Dir { get; set; }

    /// <summary>The room (floor) the piece stands in.</summary>
    public byte Room { get; set; }

    /// <summary><see cref="Index"/> of the piece this one is attached to (a tool on a work platform), or 0 for a free-standing piece.</summary>
    public ushort Parent { get; set; }

    /// <summary>The items stored in the piece (a cupboard or similar container).</summary>
    public List<CupboardItem> Contents { get; set; } = [];

    /// <summary>The offers or items held by the piece: goods for sale in a home shop, eggs in an egg machine, cars in a garage.</summary>
    public List<StockItem> Stock { get; set; } = [];

    /// <summary>Id of the garage car shown on this piece (a show platform), or 0 when it shows none.</summary>
    public byte Exhibit { get; set; }

    /// <summary>Price per draw while an egg machine is on sale; 0 when it is not selling.</summary>
    public int SalePrice { get; set; }

    /// <summary>Number of eggs fed into an egg machine.</summary>
    public byte Eggs { get; set; }

    /// <summary>The pets sleeping in this bed.</summary>
    public List<BedSleeper> Sleepers { get; set; } = [];

    /// <summary>True while the piece is kept (taken down, not standing in the house).</summary>
    public bool Retained { get; set; }

    /// <summary>When the rental of this room door started, or null for a piece that is not rented.</summary>
    public DateTime? RentStartUtc { get; set; }

    /// <summary>How many times the piece has been used (a magic sandglass).</summary>
    public byte Uses { get; set; }
}
