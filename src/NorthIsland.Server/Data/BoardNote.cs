namespace NorthIsland.Server.Data;

/// <summary>A note on a house blackboard.</summary>
public sealed class BoardNote
{
    /// <summary>The note's position on the board.</summary>
    public byte Slot { get; set; }

    /// <summary>The name of the player who wrote the note.</summary>
    public string Name { get; set; } = "";

    /// <summary>When the note was written.</summary>
    public double Time { get; set; }

    /// <summary>The note's text.</summary>
    public string Text { get; set; } = "";
}
