using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NorthIsland.GameData.Talk;

namespace NorthIsland.Server.Data;

/// <summary>Lazily loads the NPC talk lines from <c>Talk.dat</c> in the game data directory.</summary>
public sealed class TalkData
{
    /// <summary>The lazily loaded table, null when the file is missing.</summary>
    private readonly Lazy<TalkTable?> _table;

    /// <summary>Sets up the lazy load of <c>Talk.dat</c>.</summary>
    /// <param name="options">Server options giving the game data directory.</param>
    /// <param name="logger">Logger for the load message.</param>
    public TalkData(
        IOptions<ServerOptions> options,
        ILogger<TalkData> logger)
    {
        var path = Path.Combine(ServerPaths.Resolve(options.Value.GameDataDirectory), "Talk.dat");
        _table = new Lazy<TalkTable?>(() =>
        {
            if (!File.Exists(path))
                return null;
            var table = TalkTable.Open(path);
            logger.LogInformation("Loaded {Count} talk lines from {Path}", table.Count, path);
            return table;
        });
    }

    /// <summary>The talk table, loaded on first use; null when <c>Talk.dat</c> does not exist.</summary>
    public TalkTable? Table => _table.Value;
}
