using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NorthIsland.GameData.Marks;
using NorthIsland.Protocol.Messages.Events;

namespace NorthIsland.Server.Data;

/// <summary>
/// The mark table (Mark.dat) and the handling of a character's quest marks, which are counters or flags.
/// </summary>
public sealed class MarkData
{
    /// <summary>The mark table, or null when Mark.dat is missing; loaded on first access.</summary>
    private readonly Lazy<MarkTable?> _table;

    /// <summary>Sets up lazy loading of Mark.dat from the game data directory.</summary>
    /// <param name="options">Server options; they name the game data directory.</param>
    /// <param name="logger">Logs what was loaded or missing.</param>
    public MarkData(
        IOptions<ServerOptions> options,
        ILogger<MarkData> logger)
    {
        var path = Path.Combine(ServerPaths.Resolve(options.Value.GameDataDirectory), "Mark.dat");
        _table = new Lazy<MarkTable?>(() =>
        {
            if (!File.Exists(path))
            {
                logger.LogWarning("Mark data not found at {Path}; every mark is a counter", path);
                return null;
            }
            var table = MarkTable.Open(path);
            logger.LogInformation("Loaded {Count} marks from {Path}", table.Count, path);
            return table;
        });
    }

    /// <summary>Looks up the flag bit of a mark.</summary>
    /// <param name="id">The mark id.</param>
    /// <returns>The flag bit, or 0 when the mark has none or Mark.dat is missing.</returns>
    public ushort FlagBit(ushort id) => _table.Value?[id]?.FlagBit ?? 0;

    /// <summary>Looks up the stars of a mark.</summary>
    /// <param name="id">The mark id.</param>
    /// <returns>The stars the mark is worth, 0 when it has none or Mark.dat is missing.</returns>
    public byte Stars(ushort id) => _table.Value?[id]?.Stars ?? 0;

    /// <summary>Tells whether a mark is a counter.</summary>
    /// <param name="id">The mark id.</param>
    /// <returns>True when Mark.dat gives the mark no flag bit; false without Mark.dat.</returns>
    public bool IsCounter(ushort id) => _table.Value is { } t && (t[id]?.FlagBit ?? 0) == 0;

    /// <summary>
    /// Moves every counter that has a flag bit into the character's flags, setting the flag when the counter was above 0.
    /// </summary>
    /// <param name="character">The character to fix up.</param>
    public void Normalize(Character character)
    {
        foreach (var mark in character.Marks.Where(m => FlagBit(m.Id) != 0).ToList())
        {
            character.Marks.Remove(mark);
            if (mark.Count > 0 && !character.MarkFlags.Contains(mark.Id))
                character.MarkFlags.Add(mark.Id);
        }
    }

    /// <summary>Sends the character's non-zero counters and the flag bits of its set flags to the client.</summary>
    /// <param name="session">The character's session.</param>
    /// <param name="character">The character whose marks are sent.</param>
    public void SendAll(GameSession session, Character character)
    {
        var counters = character.Marks.Where(m => m.Count > 0).ToList();
        if (counters.Count > 0)
            session.Send(MarkMessages.List(counters.Select(m => (m.Id, m.Count))));
        var bits = character.MarkFlags.Select(FlagBit).Where(b => b != 0).ToList();
        if (bits.Count > 0)
            session.Send(MarkMessages.Flags(bits));
    }

    /// <summary>Tells the client that marks are gone: clears the flag bit of a flag, deletes a counter.</summary>
    /// <param name="session">The player's session.</param>
    /// <param name="ids">The ids of the removed marks.</param>
    public void SendRemoved(GameSession session, IEnumerable<ushort> ids)
    {
        foreach (var id in ids)
            session.Send(FlagBit(id) is var bit and not 0 ? MarkMessages.SetFlag(bit, false) : MarkMessages.Delete(id));
    }

    /// <summary>Sets or clears a quest flag and tells the client when it changed.</summary>
    /// <param name="session">The player's session.</param>
    /// <param name="character">The character whose flag is changed.</param>
    /// <param name="id">The mark id.</param>
    /// <param name="on">True to set the flag, false to clear it.</param>
    /// <returns>True when the mark is a flag (also when it already had the wanted state); false when it has no flag bit.</returns>
    public bool TrySetFlag(GameSession session, Character character, ushort id, bool on)
    {
        var bit = FlagBit(id);
        if (bit == 0)
            return false;
        if (on == character.MarkFlags.Contains(id))
            return true;
        if (on)
            character.MarkFlags.Add(id);
        else
            character.MarkFlags.Remove(id);
        session.Send(MarkMessages.SetFlag(bit, on));
        return true;
    }
}
