namespace NorthIsland.Server.Features.Instances;

/// <summary>A player in an instance.</summary>
/// <param name="id">The player's login id.</param>
/// <param name="name">The name of the player's character.</param>
public sealed class InstanceMember(
    int id,
    string name)
{
    /// <summary>The player's login id.</summary>
    public int Id { get; } = id;

    /// <summary>The name of the player's character.</summary>
    public string Name { get; } = name;

    /// <summary>The number of battles the player has won in the instance.</summary>
    public int Wins { get; set; }

    /// <summary>The roles of the npcs hidden for this member only.</summary>
    public HashSet<ushort> Hidden { get; } = [];
}
