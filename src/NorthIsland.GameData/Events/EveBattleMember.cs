namespace NorthIsland.GameData.Events;

/// <summary>One member of a scripted battle side.</summary>
/// <param name="Id">The member id as stored in the record.</param>
/// <param name="Unknown2">Byte after the id; its meaning is not established.</param>
public readonly record struct EveBattleMember(
    ushort Id,
    byte Unknown2);
