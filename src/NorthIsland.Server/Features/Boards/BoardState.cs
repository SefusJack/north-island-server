namespace NorthIsland.Server.Features.Boards;

/// <summary>The saved state of all notice boards.</summary>
public sealed class BoardState
{
    /// <summary>All notices of the public board and the guild boards.</summary>
    public List<Notice> Notices { get; set; } = [];

    /// <summary>The id given to the last posted notice; the next notice gets one more.</summary>
    public int LastNoticeId { get; set; }
}
