using NorthIsland.GameData.Maps;

namespace NorthIsland.Server.Features.Instances;

/// <summary>The live instances, by their map id. At most one instance exists per map copy.</summary>
public sealed class InstanceRegistry
{
    /// <summary>Guards the dictionary of instances.</summary>
    private readonly Lock _lock = new();

    /// <summary>The live instances by map id.</summary>
    private readonly SortedDictionary<ushort, Instance> _byMap = [];

    /// <summary>Finds the instance that runs on a map.</summary>
    /// <param name="mapId">The map id.</param>
    /// <returns>The instance; null when none runs on that map.</returns>
    public Instance? ByMap(int mapId)
    {
        lock (_lock)
            return mapId is >= 0 and <= ushort.MaxValue ? _byMap.GetValueOrDefault((ushort)mapId) : null;
    }

    /// <summary>Finds the instance a player is a member of.</summary>
    /// <param name="playerId">The player's login id.</param>
    /// <returns>The instance; null when the player is in none.</returns>
    public Instance? Of(int playerId)
    {
        lock (_lock)
            return _byMap.Values.FirstOrDefault(i => { lock (i.Lock) return i.Member(playerId) is not null; });
    }

    /// <summary>Lists all instances.</summary>
    /// <returns>A snapshot of the instances.</returns>
    public IReadOnlyList<Instance> All()
    {
        lock (_lock)
            return [.. _byMap.Values];
    }

    /// <summary>Lists the instances that are still waiting for members.</summary>
    /// <returns>A snapshot of the waiting instances.</returns>
    public IReadOnlyList<Instance> Waiting()
    {
        lock (_lock)
            return [.. _byMap.Values.Where(i => i.State == InstanceState.Waiting)];
    }

    /// <summary>Creates an instance on the first free map copy of a scene, with the creator as its first member.</summary>
    /// <param name="scene">The instance scene.</param>
    /// <param name="creatorId">The creator's login id.</param>
    /// <param name="creatorName">The name of the creator's character.</param>
    /// <param name="message">The message the creator gave.</param>
    /// <param name="guild">The guild the instance is restricted to; empty when it is open to everybody.</param>
    /// <param name="now">The current time.</param>
    /// <returns>The new instance; null when all copies of the scene are in use.</returns>
    public Instance? Create(
        InstanceScene scene,
        int creatorId,
        string creatorName,
        string message,
        string guild,
        DateTime now)
    {
        lock (_lock)
        {
            for (var copy = 0; copy < InstanceScene.Copies; copy++)
            {
                if (_byMap.ContainsKey(scene.MapId(copy)))
                    continue;
                var instance = new Instance(scene, copy, creatorId, creatorName, message, guild, now);
                instance.Members.Add(new InstanceMember(creatorId, creatorName));
                _byMap[instance.MapId] = instance;
                return instance;
            }
            return null;
        }
    }

    /// <summary>Removes an instance, which frees its map copy.</summary>
    /// <param name="instance">The instance.</param>
    public void Remove(Instance instance)
    {
        lock (_lock)
            _byMap.Remove(instance.MapId);
    }
}
