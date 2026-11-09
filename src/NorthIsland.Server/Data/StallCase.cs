namespace NorthIsland.Server.Data;

/// <summary>One item stack a player has put into a stall case in their house; saved with the character.</summary>
public sealed class StallCase
{
    /// <summary>The case number the stack sits in (1-based, unique per character).</summary>
    public byte Case { get; set; }

    /// <summary>Index of the furniture piece (<see cref="Furniture.Index"/>) the case stands on.</summary>
    public byte FurnIndex { get; set; }

    /// <summary>The position on that piece (1-based).</summary>
    public byte Pos { get; set; }

    /// <summary>The item id of the stacked item.</summary>
    public ushort ItemId { get; set; }

    /// <summary>How many of the item are in the case.</summary>
    public int Stock { get; set; }

    /// <summary>The price asked for one item.</summary>
    public int Price { get; set; }

    /// <summary>The wear value of the item, 1 when new.</summary>
    public byte Wear { get; set; } = 1;
}
