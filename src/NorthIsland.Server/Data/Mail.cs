namespace NorthIsland.Server.Data;

/// <summary>A letter in a character's mailbox.</summary>
public sealed class Mail
{
    /// <summary>Login id of the sender.</summary>
    public int SenderId { get; set; }

    /// <summary>Send time as the 64-bit float the client sent; it is passed back unchanged.</summary>
    public double Time { get; set; }

    /// <summary>Body of the letter.</summary>
    public string Text { get; set; } = "";
}
