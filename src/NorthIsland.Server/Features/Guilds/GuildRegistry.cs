using Microsoft.Extensions.Logging;

namespace NorthIsland.Server.Features.Guilds;

/// <summary>All guilds of the server, kept in the <c>guilds</c> document; every change saves it.</summary>
public sealed class GuildRegistry
{
    /// <summary>Name of the document the guilds are saved in.</summary>
    private const string Document = "guilds";

    /// <summary>The store the guilds are saved to.</summary>
    private readonly IDocumentStore _documents;

    /// <summary>The guilds.</summary>
    private readonly List<Guild> _guilds;

    /// <summary>Logger for failed saves.</summary>
    private readonly ILogger<GuildRegistry> _logger;

    /// <summary>Loads the saved guilds.</summary>
    /// <param name="documents">The document store the guilds are loaded from and saved to.</param>
    /// <param name="logger">The logger.</param>
    public GuildRegistry(
        IDocumentStore documents,
        ILogger<GuildRegistry> logger)
    {
        _logger = logger;
        _documents = documents;
        _guilds = documents.Load<List<Guild>>(Document) ?? [];
    }

    /// <summary>Guards the guilds and their members; hold it while changing them.</summary>
    public Lock Lock { get; } = new();

    /// <summary>The guild a player belongs to.</summary>
    /// <param name="playerId">The player's login id.</param>
    /// <returns>The guild, or null when the player is in none.</returns>
    public Guild? Of(int playerId)
    {
        lock (Lock)
            return _guilds.Find(g => g.Member(playerId) is not null);
    }

    /// <summary>A snapshot of all guilds.</summary>
    /// <returns>The guilds.</returns>
    public IReadOnlyList<Guild> All()
    {
        lock (Lock)
            return [.. _guilds];
    }

    /// <summary>Finds a guild by id.</summary>
    /// <param name="id">The guild id.</param>
    /// <returns>The guild, or null when there is none.</returns>
    public Guild? ById(uint id)
    {
        lock (Lock)
            return _guilds.Find(g => g.Id == id);
    }

    /// <summary>How many guilds there are.</summary>
    public int Count
    {
        get
        {
            lock (Lock)
                return _guilds.Count;
        }
    }

    /// <summary>Whether a guild name is in use, ignoring case.</summary>
    /// <param name="name">The name.</param>
    /// <returns>True when a guild has that name.</returns>
    public bool NameTaken(string name)
    {
        lock (Lock)
            return _guilds.Exists(g => string.Equals(g.Name, name, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>Founds a guild with the founder as lord and all five rights.</summary>
    /// <param name="name">The guild name.</param>
    /// <param name="leader">The founder's character.</param>
    /// <param name="leaderId">The founder's login id.</param>
    /// <returns>The new guild, saved.</returns>
    public Guild Create(string name, Character leader, int leaderId)
    {
        lock (Lock)
        {
            var lord = new GuildMember { Id = leaderId, Rights = [1, 1, 1, 1, 1] };
            lord.Refresh(leader);
            var guild = new Guild
            {
                Id = _guilds.Count == 0 ? 1 : _guilds.Max(g => g.Id) + 1,
                Name = name,
                LeaderId = leaderId,
                Members = [lord],
                Founded = GameClock.Now.ToOADate(),
            };
            guild.RulesSince = guild.Founded;
            _guilds.Add(guild);
            Save();
            return guild;
        }
    }

    /// <summary>Stores a new badge image and gives the guild a new badge version.</summary>
    /// <param name="guild">The guild.</param>
    /// <param name="data">The badge image bytes.</param>
    /// <returns>The new badge version.</returns>
    public byte SetBadge(Guild guild, byte[] data)
    {
        lock (Lock)
        {
            guild.Badge = [.. data];
            guild.BadgeVersion = guild.BadgeVersion == 0 ? (byte)GameRandom.Shared.Next(1, 256) : (byte)(guild.BadgeVersion % 255 + 1);
            Save();
            return guild.BadgeVersion;
        }
    }

    /// <summary>Removes a guild and saves.</summary>
    /// <param name="guild">The guild.</param>
    public void Remove(Guild guild)
    {
        lock (Lock)
        {
            _guilds.Remove(guild);
            Save();
        }
    }

    /// <summary>Saves the guilds; a failure is logged.</summary>
    public void Save()
    {
        lock (Lock)
        {
            try
            {
                _documents.Save(Document, _guilds);
            }
            catch (IOException ex)
            {
                _logger.LogError(ex, "Saving {Document} failed", Document);
            }
        }
    }
}
