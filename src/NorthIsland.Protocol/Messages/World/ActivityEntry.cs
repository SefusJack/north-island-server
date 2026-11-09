namespace NorthIsland.Protocol.Messages.World;

/// <summary>One row of the activity list that <see cref="ActivityMessages.List"/> sends to the 2014 client.</summary>
/// <param name="Flag">First byte of the row; the server always sends 0.</param>
/// <param name="Weekday">Weekday code of the row (0 for a row that is not tied to a day).</param>
/// <param name="Name">Activity name shown in the list.</param>
/// <param name="Time">Start time of day as a fraction of a day (0.5 is noon), sent as an 8-byte float.</param>
/// <param name="Minutes">Length of the activity in minutes.</param>
/// <param name="Map">Map id of the place where the activity is held.</param>
/// <param name="X">X cell of the place where the activity is held.</param>
/// <param name="Y">Y cell of the place where the activity is held.</param>
/// <param name="MinLevel">Minimum level shown for the activity.</param>
/// <param name="State">Row state: 1 not yet open, 2 open now, 3 over for today.</param>
/// <param name="Description">Description text shown for the row.</param>
public sealed record ActivityEntry(
    byte Flag,
    byte Weekday,
    string Name,
    double Time,
    ushort Minutes,
    ushort Map,
    ushort X,
    ushort Y,
    byte MinLevel,
    byte State,
    string Description);
