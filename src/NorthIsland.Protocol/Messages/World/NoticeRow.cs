namespace NorthIsland.Protocol.Messages.World;

/// <summary>One notice of a board without its text.</summary>
/// <param name="Id">Notice id.</param>
/// <param name="AuthorId">Id of the author.</param>
/// <param name="Author">Name of the author.</param>
/// <param name="Title">Title of the notice.</param>
/// <param name="Time">Time of writing, as an 8-byte float date.</param>
public sealed record NoticeRow(
    int Id,
    int AuthorId,
    string Author,
    string Title,
    double Time);
