using Microsoft.Extensions.Logging;

namespace NorthIsland.Server.Features.Boards;

/// <summary>Stores the notices of the public board and the guild boards and saves them to the document store.</summary>
public sealed class BoardRegistry
{
    /// <summary>Notices per page and the most notices a board holds, for the public board and for a guild board.</summary>
    public const int PublicPageSize = 10, PublicMaxNotices = 250, GuildPageSize = 5, GuildMaxNotices = 100;

    /// <summary>Name of the stored document.</summary>
    private const string Document = "boards";

    /// <summary>The store the board state is saved to.</summary>
    private readonly IDocumentStore _documents;

    /// <summary>The board state, guarded by <see cref="Lock"/>.</summary>
    private readonly BoardState _state;

    /// <summary>The logger.</summary>
    private readonly ILogger<BoardRegistry> _logger;

    /// <summary>Loads the saved notices, or starts empty.</summary>
    /// <param name="documents">The document store.</param>
    /// <param name="logger">The logger.</param>
    public BoardRegistry(
        IDocumentStore documents,
        ILogger<BoardRegistry> logger)
    {
        _logger = logger;
        _documents = documents;
        _state = documents.Load<BoardState>(Document) ?? new();
    }

    /// <summary>The lock that guards the state; held while changing or reading it.</summary>
    public Lock Lock { get; } = new();

    /// <summary>Lists the notices of one board, newest first.</summary>
    /// <param name="guildId">The board id: 0 for the public board, otherwise the guild id.</param>
    /// <returns>A copy of the board's notices.</returns>
    public IReadOnlyList<Notice> Notices(uint guildId)
    {
        lock (Lock)
            return [.. _state.Notices.Where(n => n.GuildId == guildId).OrderByDescending(n => n.Id)];
    }

    /// <summary>Looks up a notice by id.</summary>
    /// <param name="id">The notice id.</param>
    /// <returns>The notice, or null when it does not exist.</returns>
    public Notice? Notice(int id)
    {
        lock (Lock)
            return _state.Notices.Find(n => n.Id == id);
    }

    /// <summary>Adds a notice to a board and saves.</summary>
    /// <param name="guildId">The board id: 0 for the public board, otherwise the guild id.</param>
    /// <param name="authorId">The author's account id.</param>
    /// <param name="author">The author's character name.</param>
    /// <param name="title">The title.</param>
    /// <param name="text">The text.</param>
    /// <param name="time">The posting time as an OLE automation date.</param>
    /// <returns>The new notice, or null when the board is full.</returns>
    public Notice? Post(
        uint guildId,
        int authorId,
        string author,
        string title,
        string text,
        double time)
    {
        lock (Lock)
        {
            if (_state.Notices.Count(n => n.GuildId == guildId) >= (guildId == 0 ? PublicMaxNotices : GuildMaxNotices))
                return null;
            var notice = new Notice
            {
                Id = ++_state.LastNoticeId,
                GuildId = guildId,
                AuthorId = authorId,
                Author = author,
                Title = title,
                Text = text,
                Time = time
            };
            _state.Notices.Add(notice);
            Save();
            return notice;
        }
    }

    /// <summary>Deletes a notice and saves.</summary>
    /// <param name="notice">The notice to delete.</param>
    public void Remove(Notice notice)
    {
        lock (Lock)
        {
            _state.Notices.Remove(notice);
            Save();
        }
    }

    /// <summary>Writes the state to the document store; a failure is logged.</summary>
    private void Save()
    {
        try
        {
            _documents.Save(Document, _state);
        }
        catch (IOException ex)
        {
            _logger.LogError(ex, "Saving {Document} failed", Document);
        }
    }
}
