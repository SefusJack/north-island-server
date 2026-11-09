namespace NorthIsland.Server.Data;

/// <summary>One hotkey slot of a character.</summary>
public sealed class Hotkey
{
    /// <summary>Hotkey page.</summary>
    public byte Page { get; set; }

    /// <summary>Slot on the page.</summary>
    public byte Slot { get; set; }

    /// <summary>What the slot holds; sent back to the client as given.</summary>
    public byte Kind { get; set; }

    /// <summary>Id of the thing in the slot, interpreted by <see cref="Kind"/>.</summary>
    public ushort Id { get; set; }
}
