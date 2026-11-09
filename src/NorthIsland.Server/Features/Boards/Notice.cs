namespace NorthIsland.Server.Features.Boards;

/// <summary>A notice posted on the public board or a guild board.</summary>
public sealed class Notice
{
    /// <summary>Unique notice id, counting up as notices are posted.</summary>
    public int Id { get; set; }

    /// <summary>The board it is on: 0 for the public board, otherwise the guild id.</summary>
    public uint GuildId { get; set; }

    /// <summary>The author's account id.</summary>
    public int AuthorId { get; set; }

    /// <summary>The author's character name when it was posted.</summary>
    public string Author { get; set; } = "";

    /// <summary>The title.</summary>
    public string Title { get; set; } = "";

    /// <summary>The text.</summary>
    public string Text { get; set; } = "";

    /// <summary>The posting time as an OLE automation date.</summary>
    public double Time { get; set; }
}
