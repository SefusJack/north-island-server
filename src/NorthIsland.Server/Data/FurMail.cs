namespace NorthIsland.Server.Data;

/// <summary>A letter left in a character's furniture mailbox, delivered when the owner logs in.</summary>
public sealed class FurMail
{
    /// <summary>Login id of the sender.</summary>
    public int SenderId { get; set; }

    /// <summary>Character name of the sender at the time of sending.</summary>
    public string SenderName { get; set; } = "";

    /// <summary>Send time as the 64-bit float the client sent; it is passed back unchanged.</summary>
    public double Time { get; set; }

    /// <summary>Body of the letter.</summary>
    public string Text { get; set; } = "";
}
