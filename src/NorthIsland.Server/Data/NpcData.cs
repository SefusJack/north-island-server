using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NorthIsland.GameData.Npcs;

namespace NorthIsland.Server.Data;

/// <summary>The NPC table (Npc.dat), loaded on first use.</summary>
public sealed class NpcData
{
    /// <summary>The NPC table, or null when Npc.dat is missing; loaded on first access.</summary>
    private readonly Lazy<NpcTable?> _table;

    /// <summary>Sets up lazy loading of Npc.dat from the game data directory.</summary>
    /// <param name="options">Server options; they name the game data directory.</param>
    /// <param name="logger">Logs what was loaded or missing.</param>
    public NpcData(
        IOptions<ServerOptions> options,
        ILogger<NpcData> logger)
    {
        var path = Path.Combine(ServerPaths.Resolve(options.Value.GameDataDirectory), "Npc.dat");
        _table = new Lazy<NpcTable?>(() =>
        {
            if (!File.Exists(path))
            {
                logger.LogWarning("NPC data not found at {Path}; scene objects get no spawn state", path);
                return null;
            }
            var table = NpcTable.Open(path);
            logger.LogInformation("Loaded {Count} NPCs from {Path}", table.Count, path);
            return table;
        });
    }

    /// <summary>Tells whether an NPC type is a scene object.</summary>
    /// <param name="npcId">The NPC type id.</param>
    /// <returns>True for a scene object; false for anything else and without Npc.dat.</returns>
    public bool IsSceneObject(ushort npcId) => _table.Value?.IsSceneObject(npcId) ?? false;

    /// <summary>Looks up an NPC type; null when unknown or without Npc.dat.</summary>
    /// <param name="npcId">The NPC type id.</param>
    public NpcRecord? this[ushort npcId] => _table.Value?[npcId];

    /// <summary>Every NPC type of the table; empty without Npc.dat.</summary>
    public IEnumerable<NpcRecord> All => _table.Value?.All ?? [];
}
