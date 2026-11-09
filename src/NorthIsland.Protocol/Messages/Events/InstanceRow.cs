namespace NorthIsland.Protocol.Messages.Events;

/// <summary>One open instance in the list window.</summary>
/// <param name="MapId">Map id of the instance.</param>
/// <param name="Creator">Name of the player who created it.</param>
/// <param name="Message">The creator's message.</param>
/// <param name="Players">Number of players in it.</param>
/// <param name="Guild">Name of the guild it is restricted to, empty for none.</param>
public readonly record struct InstanceRow(
    ushort MapId,
    string Creator,
    string Message,
    byte Players,
    string Guild = "");
