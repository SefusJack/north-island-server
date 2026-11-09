namespace NorthIsland.GameData.Events;

/// <summary>The decoded event scene of one map.</summary>
public sealed record EveScene
{
    /// <summary>The map id stored in the scene header.</summary>
    public ushort HeaderMapId { get; init; }

    /// <summary>The scene format version.</summary>
    public ushort Version { get; init; }

    /// <summary>The NPC section, ordered by record index.</summary>
    public IReadOnlyList<EveNpc> Npcs { get; init; } = [];

    /// <summary>The door section, ordered by record index.</summary>
    public IReadOnlyList<EveDoor> Doors { get; init; } = [];

    /// <summary>The mine section, ordered by record index.</summary>
    public IReadOnlyList<EveMine> Mines { get; init; } = [];

    /// <summary>The goods section, ordered by record index.</summary>
    public IReadOnlyList<EveGoods> Goods { get; init; } = [];

    /// <summary>The event section, ordered by record index.</summary>
    public IReadOnlyList<EveEvent> Events { get; init; } = [];

    /// <summary>The group section, ordered by record index.</summary>
    public IReadOnlyList<EveGroup> Groups { get; init; } = [];

    /// <summary>The convey section, ordered by record index.</summary>
    public IReadOnlyList<EveConvey> Conveys { get; init; } = [];

    /// <summary>The interaction section, ordered by record index.</summary>
    public IReadOnlyList<EveInteract> Interacts { get; init; } = [];

    /// <summary>The battle section, ordered by record index.</summary>
    public IReadOnlyList<EveBattle> Battles { get; init; } = [];

    /// <summary>The "LeadEve" event section, ordered by record index.</summary>
    public IReadOnlyList<EveEvent> LeadEvents { get; init; } = [];

    /// <summary>The "LeadGroup" group section, ordered by record index.</summary>
    public IReadOnlyList<EveGroup> LeadGroups { get; init; } = [];

    /// <summary>The 11 section offsets stored after the sections; empty when the blob has too few bytes left.</summary>
    public IReadOnlyList<int> SectionOffsets { get; init; } = [];

    /// <summary>The byte position at which each of the 11 sections starts in the blob.</summary>
    public IReadOnlyList<int> SectionStarts { get; init; } = [];

    /// <summary>The number of bytes left in the blob after the offsets table.</summary>
    public int TrailingBytes { get; init; }

    /// <summary>Finds an NPC by its 1-based position in the NPC section.</summary>
    /// <param name="roleIndex">The runtime role index.</param>
    /// <returns>The NPC, or null when the index is out of range.</returns>
    public EveNpc? NpcByRoleIndex(int roleIndex) =>
        roleIndex >= 1 && roleIndex <= Npcs.Count ? Npcs[roleIndex - 1] : null;

    /// <summary>Finds a door by its record index.</summary>
    /// <param name="doorIndex">The door record index.</param>
    /// <returns>The door, or null.</returns>
    public EveDoor? DoorByIndex(int doorIndex) => Doors.FirstOrDefault(d => d.Index == doorIndex);

    /// <summary>Finds an event by its record index.</summary>
    /// <param name="eventIndex">The event record index.</param>
    /// <returns>The event, or null.</returns>
    public EveEvent? EventByIndex(int eventIndex) => Events.FirstOrDefault(e => e.Index == eventIndex);

    /// <summary>Finds a convey by its record index.</summary>
    /// <param name="conveyIndex">The convey record index.</param>
    /// <returns>The convey, or null.</returns>
    public EveConvey? ConveyByIndex(int conveyIndex) => Conveys.FirstOrDefault(c => c.Index == conveyIndex);

    /// <summary>Returns the events an object runs, skipping ids with no event.</summary>
    /// <param name="obj">The placed object.</param>
    /// <returns>The events in the order of the object's event ids.</returns>
    public IEnumerable<EveEvent> EventsOf(EvePlacedObject obj)
    {
        ArgumentNullException.ThrowIfNull(obj);
        return obj.EventIds.Select(id => EventByIndex(id)).OfType<EveEvent>();
    }

    /// <summary>Returns the conveys a door can warp to, found through the warp results of its events.</summary>
    /// <param name="door">The door.</param>
    /// <returns>The conveys whose index a warp result names and that exist in this scene.</returns>
    public IEnumerable<EveConvey> DoorDestinations(EveDoor door)
    {
        foreach (var result in EventsOf(door).SelectMany(e => e.Conditions).SelectMany(c => c.Results))
            if (result.WarpConvey() is { } index && ConveyByIndex(index) is { } convey)
                yield return convey;
    }
}
