namespace NorthIsland.Server.World;

/// <summary>The teams on the server: each a leader and up to 3 members, kept by player id.</summary>
public sealed class Teams
{
    /// <summary>The most members a team has besides its leader.</summary>
    public const int MaxMembers = 3;

    /// <summary>The members of each team by leader id.</summary>
    private readonly Dictionary<int, List<int>> _byLeader = [];

    /// <summary>Guards the teams.</summary>
    private readonly Lock _lock = new();

    /// <summary>Finds the leader of the team a player is in.</summary>
    /// <param name="playerId">The player id.</param>
    /// <returns>The leader's id (the player's own for a leader), or 0 when the player is in no team.</returns>
    public int LeaderOf(int playerId)
    {
        lock (_lock)
        {
            if (_byLeader.ContainsKey(playerId))
                return playerId;
            foreach (var (leader, members) in _byLeader)
            {
                if (members.Contains(playerId))
                    return leader;
            }
            return 0;
        }
    }

    /// <summary>Whether a player leads a team.</summary>
    /// <param name="playerId">The player id.</param>
    /// <returns>True when the player is a leader.</returns>
    public bool IsLeader(int playerId)
    {
        lock (_lock)
            return _byLeader.ContainsKey(playerId);
    }

    /// <summary>The members of a leader's team, not counting the leader.</summary>
    /// <param name="leaderId">The leader's player id.</param>
    /// <returns>The member ids, empty when there is no such team.</returns>
    public IReadOnlyList<int> MembersOf(int leaderId)
    {
        lock (_lock)
            return _byLeader.TryGetValue(leaderId, out var m) ? [.. m] : [];
    }

    /// <summary>The whole team a player is in, the leader first.</summary>
    /// <param name="playerId">The player id.</param>
    /// <returns>The leader and member ids, empty when the player is in no team.</returns>
    public IReadOnlyList<int> TeamOf(int playerId)
    {
        var leader = LeaderOf(playerId);
        return leader == 0 ? [] : [leader, .. MembersOf(leader)];
    }

    /// <summary>Adds a player to a leader's team, creating it if needed.</summary>
    /// <param name="leaderId">The leader's player id.</param>
    /// <param name="memberId">The player to add.</param>
    /// <returns>True when added; false when the player is already in a team, the leader is a member elsewhere or the team is full.</returns>
    public bool Add(int leaderId, int memberId)
    {
        lock (_lock)
        {
            if (leaderId == memberId || LeaderOfUnlocked(memberId) != 0)
                return false;
            var leaderOf = LeaderOfUnlocked(leaderId);
            if (leaderOf != 0 && leaderOf != leaderId)
                return false;
            if (!_byLeader.TryGetValue(leaderId, out var members))
                _byLeader[leaderId] = members = [];
            if (members.Count >= MaxMembers)
                return false;
            members.Add(memberId);
            return true;
        }
    }

    /// <summary>Removes a player from their team: a leader's removal disbands the team, and a team left empty is dropped.</summary>
    /// <param name="playerId">The player id.</param>
    /// <returns>The leader and the members as they were (including the removed member), with leader 0 when the player was in no team.</returns>
    public (int Leader, IReadOnlyList<int> Members) Remove(int playerId)
    {
        lock (_lock)
        {
            if (_byLeader.Remove(playerId, out var members))
                return (playerId, members);
            foreach (var (leader, list) in _byLeader)
            {
                if (!list.Remove(playerId))
                    continue;
                var before = (leader, (IReadOnlyList<int>)[.. list, playerId]);
                if (list.Count == 0)
                    _byLeader.Remove(leader);
                return before;
            }
            return (0, []);
        }
    }

    /// <summary>Makes a member the leader; the old leader takes the member's place in the list.</summary>
    /// <param name="leaderId">The current leader's player id.</param>
    /// <param name="memberId">The member to promote.</param>
    /// <returns>True when promoted; false when the member is not in that leader's team.</returns>
    public bool Promote(int leaderId, int memberId)
    {
        lock (_lock)
        {
            if (!_byLeader.TryGetValue(leaderId, out var members) || members.IndexOf(memberId) is var i && i < 0)
                return false;
            _byLeader.Remove(leaderId);
            members[i] = leaderId;
            _byLeader[memberId] = members;
            return true;
        }
    }

    /// <summary>All teams.</summary>
    /// <returns>Each team's leader and members.</returns>
    public IReadOnlyList<(int Leader, IReadOnlyList<int> Members)> All()
    {
        lock (_lock)
            return [.. _byLeader.Select(kv => (kv.Key, (IReadOnlyList<int>)[.. kv.Value]))];
    }

    /// <summary>Finds the leader of a player's team; for callers that already hold the lock.</summary>
    /// <param name="playerId">The player id.</param>
    /// <returns>The leader's id, or 0 when the player is in no team.</returns>
    private int LeaderOfUnlocked(int playerId)
    {
        if (_byLeader.ContainsKey(playerId))
            return playerId;
        foreach (var (leader, members) in _byLeader)
        {
            if (members.Contains(playerId))
                return leader;
        }
        return 0;
    }
}
