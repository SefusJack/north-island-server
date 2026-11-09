using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;
using NorthIsland.Protocol.IO;
using NorthIsland.Protocol.Messages.World;
using NorthIsland.Protocol.Text;

namespace NorthIsland.Server.Features.Boards;

/// <summary>Handles the notice boards (the public board and each guild's board) and opens the Top 100 window.</summary>
/// <param name="boards">The notice store.</param>
/// <param name="guilds">The guild registry, for the player's guild and its officers.</param>
/// <param name="mail">The mail handlers, used to deliver replies to a notice's author.</param>
/// <param name="rankings">The ranking handlers, which refresh the Top 100 lists.</param>
/// <param name="logger">The logger.</param>
public sealed class BoardHandlers(
    BoardRegistry boards,
    GuildRegistry guilds,
    FriendHandlers mail,
    RankingHandlers rankings,
    ILogger<BoardHandlers> logger)
{
    /// <summary>Service id of the notice board.</summary>
    public const int NoticeBoardService = 15;

    /// <summary>Service id of the Top 100 window.</summary>
    public const int Top100Service = 19;

    /// <summary>The notice each session opened last, which a reply is addressed to.</summary>
    private readonly ConditionalWeakTable<GameSession, StrongBox<int>> _opened = [];

    /// <summary>Shows the notice board window.</summary>
    /// <param name="session">The player's session.</param>
    public void OpenNoticeBoard(GameSession session)
    {
        session.Send(BoardMessages.NoticeBoardShow());
        logger.LogInformation("[{Session}] notice board", session.Id);
    }

    /// <summary>Refreshes the ranking lists and shows the Top 100 window.</summary>
    /// <param name="session">The player's session.</param>
    public void OpenTop100(GameSession session)
    {
        rankings.Refresh(session);
        session.Send(BoardMessages.Top100Show());
        logger.LogInformation("[{Session}] top 100", session.Id);
    }

    /// <summary>Finds the board a player posts to.</summary>
    /// <param name="playerId">The player's account id.</param>
    /// <param name="guild">True for the guild board, false for the public board.</param>
    /// <returns>0 for the public board, the guild id for a guild board, null when the player is in no guild.</returns>
    private uint? BoardOf(int playerId, bool guild) => !guild ? 0 : guilds.Of(playerId)?.Id;

    /// <summary>Sends the requested page of the board's notices.</summary>
    /// <param name="session">The player's session.</param>
    /// <param name="body">The packet body: the page number.</param>
    /// <param name="guild">True for the guild board, false for the public board.</param>
    internal ValueTask OnList(GameSession session, ReadOnlyMemory<byte> body, bool guild)
    {
        if (InWorld(session) is not (_, var id) || body.Length < 1 || BoardOf(id, guild) is not { } board)
            return ValueTask.CompletedTask;
        SendPage(session, board, guild, Math.Max((byte)1, body.Span[0])); // page
        return ValueTask.CompletedTask;
    }

    /// <summary>Sends the page count information and the notice rows of one page.</summary>
    /// <param name="session">The player's session.</param>
    /// <param name="board">The board id: 0 for the public board, otherwise the guild id.</param>
    /// <param name="guild">True for the guild board.</param>
    /// <param name="page">The 1-based page number, limited to the last page.</param>
    private void SendPage(GameSession session, uint board, bool guild, byte page)
    {
        var size = guild ? BoardRegistry.GuildPageSize : BoardRegistry.PublicPageSize;
        var notices = boards.Notices(board);
        var pages = Math.Max(1, (notices.Count + size - 1) / size);
        page = (byte)Math.Min(page, pages);
        session.Send(BoardMessages.NoticePageInfo(guild, notices.Count, page));
        session.Send(BoardMessages.NoticeList(guild, notices.Skip((page - 1) * size).Take(size).Select(Row)));
    }

    /// <summary>Converts a notice to the row the list packet carries.</summary>
    /// <param name="n">The notice.</param>
    /// <returns>The list row without the text.</returns>
    private static NoticeRow Row(Notice n) => new(n.Id, n.AuthorId, n.Author, n.Title, n.Time);

    /// <summary>Looks up the notice a request names.</summary>
    /// <param name="body">The packet body that starts with the notice id.</param>
    /// <param name="board">The board the notice must belong to.</param>
    /// <returns>The notice, or null when it does not exist or is on another board.</returns>
    private Notice? Find(ReadOnlyMemory<byte> body, uint board) =>
        body.Length >= 4 && boards.Notice(new PacketReader(body.Span).I32()) is { } n && n.GuildId == board ? n : null;

    /// <summary>Sends the content of a notice and remembers it as the one the player has open.</summary>
    /// <param name="session">The player's session.</param>
    /// <param name="body">The packet body: the notice id.</param>
    /// <param name="guild">True for the guild board.</param>
    internal ValueTask OnOpen(GameSession session, ReadOnlyMemory<byte> body, bool guild)
    {
        if (InWorld(session) is not (_, var id) || BoardOf(id, guild) is not { } board)
            return ValueTask.CompletedTask;
        if (Find(body, board) is not { } notice)
        {
            session.Send(BoardMessages.NoticeNotFound());
            return ValueTask.CompletedTask;
        }
        _opened.AddOrUpdate(session, new StrongBox<int>(notice.Id));
        session.Send(BoardMessages.NoticeContent(guild, Row(notice), notice.Text));
        return ValueTask.CompletedTask;
    }

    /// <summary>Deletes a notice if the player wrote it or leads or deputizes in the guild, then sends the current page again.</summary>
    /// <param name="session">The player's session.</param>
    /// <param name="body">The packet body: the notice id and optionally the page to show.</param>
    /// <param name="guild">True for the guild board.</param>
    internal ValueTask OnDelete(GameSession session, ReadOnlyMemory<byte> body, bool guild)
    {
        if (InWorld(session) is not (var character, var id) || BoardOf(id, guild) is not { } board)
            return ValueTask.CompletedTask;
        if (Find(body, board) is not { } notice)
        {
            session.Send(BoardMessages.NoticeNotFound());
            return ValueTask.CompletedTask;
        }
        var g = guild ? guilds.ById(board) : null;
        if (notice.AuthorId != id && (g is null || (g.LeaderId != id && !g.Vices.Contains(id))))
        {
            session.Send(BoardMessages.NoticeNoRight());
            return ValueTask.CompletedTask;
        }
        boards.Remove(notice);
        if (guild)
            session.Send(BoardMessages.GuildNoticeDeleted(character.Name, notice.Title));
        var page = body.Length >= 8 ? (byte)Math.Clamp(new PacketReader(body.Span[4..]).I32(), 1, 25) : (byte)1;
        SendPage(session, board, guild, page);
        logger.LogInformation("[{Session}] {Name} deletes notice {Id} '{Title}'", session.Id, character.Name, notice.Id, notice.Title);
        return ValueTask.CompletedTask;
    }

    /// <summary>Answers a request to write a notice: shows the form, or tells that the board is full.</summary>
    /// <param name="session">The player's session.</param>
    /// <param name="guild">True for the guild board.</param>
    internal ValueTask OnAskAdd(GameSession session, bool guild)
    {
        if (InWorld(session) is not (_, var id) || BoardOf(id, guild) is not { } board)
            return ValueTask.CompletedTask;
        var full = boards.Notices(board).Count >= (guild ? BoardRegistry.GuildMaxNotices : BoardRegistry.PublicMaxNotices);
        session.Send(full ? BoardMessages.NoticeFull() : BoardMessages.NoticeShowSend(guild));
        return ValueTask.CompletedTask;
    }

    /// <summary>Posts a new notice from the submitted form and shows the first page.</summary>
    /// <param name="session">The player's session.</param>
    /// <param name="body">The packet body with the title and text.</param>
    /// <param name="guild">True for the guild board.</param>
    internal ValueTask OnSubmit(GameSession session, ReadOnlyMemory<byte> body, bool guild)
    {
        if (InWorld(session) is not (var character, var id) || BoardOf(id, guild) is not { } board)
            return ValueTask.CompletedTask;
        var r = new PacketReader(body.Span);
        var title = GameEncoding.GetString(r.Bytes(Math.Max(0, r.I32()))).Trim();
        var text = GameEncoding.GetString(r.Bytes(Math.Max(0, r.I32()))).Trim();
        if (title.Length == 0 || text.Length == 0)
            return ValueTask.CompletedTask;
        if (boards.Post(board, id, character.Name, title, text, GameClock.Now.ToOADate()) is null)
        {
            session.Send(BoardMessages.NoticeFull());
            return ValueTask.CompletedTask;
        }
        SendPage(session, board, guild, 1);
        logger.LogInformation("[{Session}] {Name} posts '{Title}' on the {Board} board", session.Id, character.Name, title, guild ? "guild" : "public");
        return ValueTask.CompletedTask;
    }

    /// <summary>Delivers a reply as a mail to the author of the notice the player opened last.</summary>
    /// <param name="session">The player's session.</param>
    /// <param name="body">The packet body with the author's name and the reply text.</param>
    internal ValueTask OnReply(GameSession session, ReadOnlyMemory<byte> body)
    {
        if (InWorld(session) is not (var character, var id) || body.Length < 5)
            return ValueTask.CompletedTask;
        var r = new PacketReader(body.Span);
        r.I32();
        r.PStr8();
        var text = r.RestString();
        if (!_opened.TryGetValue(session, out var opened) || boards.Notice(opened.Value) is not { } notice || text.Length == 0)
            return ValueTask.CompletedTask;
        logger.LogInformation("[{Session}] {Name} answers notice {Id} of {Author}", session.Id, character.Name, notice.Id, notice.Author);
        return new ValueTask(mail.DeliverAsync(id, notice.AuthorId, $"[{notice.Title}] {text}"));
    }

    /// <summary>Gets the active character and account id of a session that is in the world.</summary>
    /// <param name="session">The player's session.</param>
    /// <returns>The character and account id, or null when the session is not in the world.</returns>
    private static (Character Character, int Id)? InWorld(GameSession session)
    {
        var character = session.ActiveCharacter;
        var id = session.Account?.LoginId ?? 0;
        return session.State == SessionState.InWorld && character is not null && id != 0 ? (character, id) : null;
    }
}
