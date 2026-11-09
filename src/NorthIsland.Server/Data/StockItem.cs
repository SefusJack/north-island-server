namespace NorthIsland.Server.Data;

/// <summary>An item offered on a house shop shelf or held in an egg machine cell; saved with the furniture piece.</summary>
public sealed class StockItem
{
    /// <summary>The id of this offer, unique among the character's furniture.</summary>
    public byte Id { get; set; }

    /// <summary>The shelf row or machine cell the item sits in (1-based).</summary>
    public byte Pos { get; set; }

    /// <summary>The item id of the stacked item.</summary>
    public ushort ItemId { get; set; }

    /// <summary>How many of the item are in the stack.</summary>
    public byte Count { get; set; }

    /// <summary>The wear value of the item, 1 when new.</summary>
    public byte Wear { get; set; } = 1;

    /// <summary>The inserted (socketed) item id, copied from the bag stack.</summary>
    public ushort Insert { get; set; }

    /// <summary>The fusion value, copied from the bag stack.</summary>
    public byte Fusion { get; set; }

    /// <summary>The pose value, copied from the bag stack.</summary>
    public ushort Pose { get; set; }

    /// <summary>The box item id, copied from the bag stack.</summary>
    public ushort BoxItem { get; set; }

    /// <summary>The used value, copied from the bag stack.</summary>
    public int Used { get; set; }

    /// <summary>The extra value, copied from the bag stack.</summary>
    public ushort Extra { get; set; }

    /// <summary>The price asked for the stack on a shop shelf; 0 in an egg machine.</summary>
    public int Price { get; set; }

    /// <summary>The bag slot the item was offered from; the shop handler refuses a second offer from the same slot.</summary>
    public byte BagCell { get; set; }
}
