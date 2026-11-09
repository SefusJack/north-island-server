namespace NorthIsland.Server.Data;

/// <summary>A tent a character has pitched on a map.</summary>
public sealed class PitchedTent
{
    /// <summary>Map the tent stands on.</summary>
    public ushort MapId { get; set; }

    /// <summary>X position of the tent on the map.</summary>
    public int X { get; set; }

    /// <summary>Y position of the tent on the map.</summary>
    public int Y { get; set; }

    /// <summary>Item id of the tent that was pitched.</summary>
    public ushort ItemId { get; set; }

    /// <summary>First style value of the tent, set by the first palette.</summary>
    public byte Kind1 { get; set; }

    /// <summary>Second style value of the tent, set by the second palette.</summary>
    public byte Kind2 { get; set; }

    /// <summary>True while the tent is locked against visitors other than its owner.</summary>
    public bool Locked { get; set; }
}
