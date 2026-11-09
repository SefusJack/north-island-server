namespace NorthIsland.GameData.Events;

/// <summary>One 92-byte item sub-record of an NPC.</summary>
/// <param name="Slot">The slot, 1..10.</param>
/// <param name="Used">The "used" flag byte.</param>
/// <param name="Raw">The whole 92 bytes, undecoded.</param>
public sealed record EveNpcItem(
    byte Slot,
    byte Used,
    byte[] Raw);
