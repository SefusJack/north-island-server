namespace NorthIsland.Server.Features.Guilds;

/// <summary>A letter in a guild mailbox.</summary>
public sealed class GuildMail
{
    /// <summary>The most letters one member's mailbox holds.</summary>
    public const int Capacity = 20;

    /// <summary>Login id of the sender.</summary>
    public int From { get; set; }

    /// <summary>When the letter was sent, as an OLE automation date.</summary>
    public double Time { get; set; }

    /// <summary>The letter text.</summary>
    public string Text { get; set; } = "";
}
