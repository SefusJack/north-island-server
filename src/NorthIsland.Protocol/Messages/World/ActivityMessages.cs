using NorthIsland.Protocol.IO;
using NorthIsland.Protocol.Opcodes;

namespace NorthIsland.Protocol.Messages.World;

/// <summary>Builders for the activity list packets (action 90) that the 2014 client shows behind its function bar.</summary>
public static class ActivityMessages
{
    /// <summary>Builds the <c>90/0</c> packet with the whole activity list; only the first 30 entries are written.</summary>
    /// <param name="weekday">Weekday code of the current day.</param>
    /// <param name="version">Version number of the list; it changes when the list content changes.</param>
    /// <param name="entries">The rows of the list.</param>
    /// <returns>The packet.</returns>
    public static byte[] List(byte weekday, int version, IEnumerable<ActivityEntry> entries)
    {
        var w = new PacketWriter(Action90.Code, Action90.List).U8(weekday).I32(version);
        foreach (var e in entries.Take(30))
            w.U8(e.Flag).U8(e.Weekday).PStr8(e.Name).F64(e.Time).U16(e.Minutes).U16(e.Map).U16(e.X).U16(e.Y).U8(e.MinLevel).U8(e.State).PStr8(e.Description);
        return w.ToArray();
    }

    /// <summary>Builds the <c>90/1</c> packet that changes the state of single rows of the list the client already has.</summary>
    /// <param name="pairs">Row id (1-based position in the list) and the new state of that row.</param>
    /// <param name="resetWeekday">When set, a leading entry with id 0 and this weekday code is written first.</param>
    /// <returns>The packet.</returns>
    public static byte[] State(IEnumerable<(byte Id, byte State)> pairs, byte? resetWeekday = null)
    {
        var w = new PacketWriter(Action90.Code, Action90.State);
        if (resetWeekday is { } weekday)
            w.U8(0).U8(weekday);
        foreach (var (id, state) in pairs)
            w.U8(id).U8(state);
        return w.ToArray();
    }

    /// <summary>Builds the <c>90/2</c> packet with a notice code for the activity window.</summary>
    /// <param name="kind">Notice code; the server sends 1.</param>
    /// <returns>The packet.</returns>
    public static byte[] Notice(byte kind) => new PacketWriter(Action90.Code, Action90.Notice).U8(kind).ToArray();
}
