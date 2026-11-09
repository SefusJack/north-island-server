namespace NorthIsland.GameData.Events;

/// <summary>One member of a group.</summary>
/// <param name="Id">The member id.</param>
/// <param name="Chance">The chance byte stored with the member.</param>
public readonly record struct EveGroupMember(
    ushort Id,
    byte Chance);
