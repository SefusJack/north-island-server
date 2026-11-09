using Microsoft.Extensions.Logging;

namespace NorthIsland.Server.Features.Boards;

/// <summary>Stores the PK wins and the quiz and event ranking places and saves them to the document store.</summary>
public sealed class RankingRegistry
{
    /// <summary>The number of places in the PK ranking and in the event ranking.</summary>
    public const int PkSize = 10, EventSize = 10;

    /// <summary>The number of places in the quiz ranking.</summary>
    public const int QuizSize = 15;

    /// <summary>Name of the stored document.</summary>
    private const string Document = "rankings";

    /// <summary>The store the ranking state is saved to.</summary>
    private readonly IDocumentStore _documents;

    /// <summary>The ranking state, guarded by <c>_lock</c>.</summary>
    private readonly RankingState _state;

    /// <summary>The logger.</summary>
    private readonly ILogger<RankingRegistry> _logger;

    /// <summary>The lock that guards the state.</summary>
    private readonly Lock _lock = new();

    /// <summary>Loads the saved rankings, or starts empty.</summary>
    /// <param name="documents">The document store.</param>
    /// <param name="logger">The logger.</param>
    public RankingRegistry(
        IDocumentStore documents,
        ILogger<RankingRegistry> logger)
    {
        _logger = logger;
        _documents = documents;
        _state = documents.Load<RankingState>(Document) ?? new();
    }

    /// <summary>Raised after a ranking has changed.</summary>
    public event Action? Changed;

    /// <summary>Adds a PK win for a player and saves.</summary>
    /// <param name="playerId">The player's account id.</param>
    /// <param name="name">The player's character name, stored as their latest name.</param>
    public void AddPkWin(int playerId, string name)
    {
        lock (_lock)
        {
            var record = _state.Pk.Find(r => r.PlayerId == playerId);
            if (record is null)
                _state.Pk.Add(record = new PkRecord { PlayerId = playerId });
            record.Name = name;
            record.Wins++;
            Save();
        }
        Changed?.Invoke();
    }

    /// <summary>Lists the players with the most PK wins; ties keep the order of first win.</summary>
    /// <returns>At most <see cref="PkSize"/> records, best first.</returns>
    public IReadOnlyList<PkRecord> PkTop()
    {
        lock (_lock)
            return [.. _state.Pk.Select((r, i) => (r, i)).OrderByDescending(x => x.r.Wins).ThenBy(x => x.i).Take(PkSize).Select(x => x.r)];
    }

    /// <summary>Replaces the quiz ranking and saves.</summary>
    /// <param name="places">The names in rank order; empty names are dropped and the list is cut to <see cref="QuizSize"/>.</param>
    public void SetQuiz(IEnumerable<string> places)
    {
        lock (_lock)
        {
            _state.Quiz = [.. places.Where(n => n.Length > 0).Take(QuizSize)];
            Save();
        }
        Changed?.Invoke();
    }

    /// <summary>Replaces the event ranking and saves.</summary>
    /// <param name="places">The names in rank order; empty names are dropped and the list is cut to <see cref="EventSize"/>.</param>
    public void SetCompass(IEnumerable<string> places)
    {
        lock (_lock)
        {
            _state.Compass = [.. places.Where(n => n.Length > 0).Take(EventSize)];
            Save();
        }
        Changed?.Invoke();
    }

    /// <summary>Gets the quiz ranking.</summary>
    /// <returns>A copy of the names in rank order.</returns>
    public IReadOnlyList<string> Quiz()
    {
        lock (_lock)
            return [.. _state.Quiz];
    }

    /// <summary>Gets the event ranking.</summary>
    /// <returns>A copy of the names in rank order.</returns>
    public IReadOnlyList<string> Compass()
    {
        lock (_lock)
            return [.. _state.Compass];
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
