using NorthIsland.Protocol.IO;
using NorthIsland.Protocol.Opcodes;
using NorthIsland.Protocol.Text;

namespace NorthIsland.Protocol.Messages.World;

/// <summary>Builders for the notice board packets (action 82) and the Top 100 ranking packets (action 84).</summary>
public static class BoardMessages
{
    /// <summary>Builds the packet that opens the notice board window.</summary>
    /// <returns>The packet.</returns>
    public static byte[] NoticeBoardShow() => new PacketWriter(Action82.Code, Action82.Show).ToArray();

    /// <summary>Builds the packet that opens the Top 100 window.</summary>
    /// <returns>The packet.</returns>
    public static byte[] Top100Show() => new PacketWriter(Action84.Code, Action84.Request).ToArray();

    /// <summary>Builds the packet with one page of notice rows, for the public board or the guild board.</summary>
    /// <param name="guild">True for the guild board, false for the public board.</param>
    /// <param name="rows">The notices of the page.</param>
    /// <returns>The packet.</returns>
    public static byte[] NoticeList(bool guild, IEnumerable<NoticeRow> rows)
    {
        var w = new PacketWriter(Action82.Code, guild ? Action82.GuildRows : Action82.Rows);
        foreach (var r in rows)
            Long(Long(w, r.Author), r.Title).F64(r.Time).I32(r.AuthorId).I32(r.Id);
        return w.ToArray();
    }

    /// <summary>Builds the packet with the number of notices and the current page of a board.</summary>
    /// <param name="guild">True for the guild board, false for the public board.</param>
    /// <param name="count">Total number of notices on the board.</param>
    /// <param name="page">The page number shown.</param>
    /// <returns>The packet.</returns>
    public static byte[] NoticePageInfo(bool guild, int count, byte page) =>
        new PacketWriter(Action82.Code, guild ? Action82.GuildPageInfo : Action82.PageInfo).I32(count).U8(page).ToArray();

    /// <summary>Builds the packet with the full text of one notice.</summary>
    /// <param name="guild">True for the guild board, false for the public board.</param>
    /// <param name="row">The notice row with author and title.</param>
    /// <param name="text">The body text of the notice.</param>
    /// <returns>The packet.</returns>
    public static byte[] NoticeContent(bool guild, NoticeRow row, string text)
    {
        var w = new PacketWriter(Action82.Code, guild ? Action82.GuildContent : Action82.Content);
        var bytes = GameEncoding.GetBytes(text);
        return Long(Long(w, row.Author), row.Title).F64(row.Time).I32(bytes.Length).I32(row.AuthorId).Bytes(bytes).ToArray();
    }

    /// <summary>Builds the packet that opens the form for writing a notice.</summary>
    /// <param name="guild">True to write on the guild board (sends 2), false for the public board (sends 1).</param>
    /// <returns>The packet.</returns>
    public static byte[] NoticeShowSend(bool guild) => new PacketWriter(Action82.Code, Action82.ShowSend).U8(guild ? (byte)2 : (byte)1).ToArray();

    /// <summary>Builds the packet that tells the player the board is full.</summary>
    /// <returns>The packet.</returns>
    public static byte[] NoticeFull() => new PacketWriter(Action82.Code, Action82.Full).ToArray();

    /// <summary>Builds the packet that tells the player the notice does not exist (any more).</summary>
    /// <returns>The packet.</returns>
    public static byte[] NoticeNotFound() => new PacketWriter(Action82.Code, Action82.NotFound).ToArray();

    /// <summary>Builds the packet that tells the player they have no right to do that on the board.</summary>
    /// <returns>The packet.</returns>
    public static byte[] NoticeNoRight() => new PacketWriter(Action82.Code, Action82.NoRight).ToArray();

    /// <summary>Builds the packet that reports a deleted guild notice by name and title.</summary>
    /// <param name="name">Name of the character who deleted the notice.</param>
    /// <param name="title">Title of the deleted notice.</param>
    /// <returns>The packet.</returns>
    public static byte[] GuildNoticeDeleted(string name, string title) =>
        Long(Long(new PacketWriter(Action82.Code, Action82.GuildDeleted), name), title).ToArray();

    /// <summary>Builds the first Top 100 reply, which gives the number of entries of each ranking.</summary>
    /// <param name="level">Number of entries in the level ranking.</param>
    /// <param name="money">Number of entries in the money ranking.</param>
    /// <param name="qa">Number of entries in the quiz ranking.</param>
    /// <param name="pk">Number of entries in the PK ranking.</param>
    /// <param name="event1">Number of entries in the first event ranking.</param>
    /// <param name="event2">Number of entries in the second event ranking.</param>
    /// <returns>The packet.</returns>
    public static byte[] RankCounts(
        int level,
        int money,
        byte qa,
        byte pk,
        byte event1,
        byte event2) =>
            new PacketWriter(Action84.Code, Action84.List).U8(0).U8(0).I32(level).I32(money).U8(qa).U8(pk).U8(event1).U8(event2).ToArray();

    /// <summary>Builds the page of the level ranking; page 1 also carries the player's own rank and a header text.</summary>
    /// <param name="page">Page number.</param>
    /// <param name="rows">Job, level and name of each ranked player on the page.</param>
    /// <param name="myRank">The asking player's own rank (written on page 1 only).</param>
    /// <param name="header">Header text (written on page 1 only).</param>
    /// <returns>The packet.</returns>
    public static byte[] LevelRanks(byte page, IEnumerable<LevelRankRow> rows, int myRank, string header)
    {
        var w = new PacketWriter(Action84.Code, Action84.List).U8(1).U8(page);
        foreach (var r in rows)
            w.U8(r.Job).U8(r.Level).PStr8(r.Name);
        if (page == 1)
            w.I32(myRank).RawString(header);
        return w.ToArray();
    }

    /// <summary>Builds the page of the money ranking; page 1 also carries the player's own rank.</summary>
    /// <param name="page">Page number.</param>
    /// <param name="names">Names of the ranked players on the page.</param>
    /// <param name="myRank">The asking player's own rank (written on page 1 only).</param>
    /// <returns>The packet.</returns>
    public static byte[] MoneyRanks(byte page, IEnumerable<string> names, int myRank)
    {
        var w = new PacketWriter(Action84.Code, Action84.List).U8(2).U8(page);
        foreach (var n in names)
            w.PStr8(n);
        if (page == 1)
            w.I32(myRank);
        return w.ToArray();
    }

    /// <summary>Builds an empty page for a ranking that has no entries.</summary>
    /// <param name="kind">Ranking kind byte.</param>
    /// <param name="page">Page number.</param>
    /// <returns>The packet.</returns>
    public static byte[] EmptyRanks(byte kind, byte page) => new PacketWriter(Action84.Code, Action84.List).U8(kind).U8(page).U8(0).ToArray();

    /// <summary>Builds the page of the quiz (Q&amp;A) ranking.</summary>
    /// <param name="page">Page number.</param>
    /// <param name="names">Names of the ranked players on the page.</param>
    /// <returns>The packet.</returns>
    public static byte[] QuizRanks(byte page, IEnumerable<string> names)
    {
        var w = new PacketWriter(Action84.Code, Action84.List).U8(3).U8(page);
        foreach (var n in names)
            w.PStr8(n);
        return w.ToArray();
    }

    /// <summary>Builds the page of the PK ranking.</summary>
    /// <param name="page">Page number.</param>
    /// <param name="rows">Place (1 to 10) and name of each ranked player.</param>
    /// <returns>The packet.</returns>
    public static byte[] PkRanks(byte page, IReadOnlyCollection<(byte Place, string Name)> rows)
    {
        var w = new PacketWriter(Action84.Code, Action84.List).U8(4).U8(page).U8((byte)rows.Count);
        foreach (var (place, name) in rows)
            w.U8(place).PStr8(name);
        return w.ToArray();
    }

    /// <summary>Builds the page of an event ranking.</summary>
    /// <param name="kind">Ranking kind byte (5 or 6).</param>
    /// <param name="page">Page number.</param>
    /// <param name="serverCode">Server code written before each name.</param>
    /// <param name="rows">Place (1 to 10) and name of each ranked player.</param>
    /// <returns>The packet.</returns>
    public static byte[] EventRanks(byte kind, byte page, byte serverCode, IReadOnlyCollection<(byte Place, string Name)> rows)
    {
        var w = new PacketWriter(Action84.Code, Action84.List).U8(kind).U8(page).U8((byte)rows.Count);
        foreach (var (place, name) in rows)
            w.U8(place).U8(serverCode).PStr8(name);
        return w.ToArray();
    }

    /// <summary>Writes a string as a 4-byte length followed by its bytes in the game encoding.</summary>
    /// <param name="w">The writer to append to.</param>
    /// <param name="text">The text to write.</param>
    /// <returns>The same writer, for chaining.</returns>
    private static PacketWriter Long(PacketWriter w, string text)
    {
        var bytes = GameEncoding.GetBytes(text);
        return w.I32(bytes.Length).Bytes(bytes);
    }
}
