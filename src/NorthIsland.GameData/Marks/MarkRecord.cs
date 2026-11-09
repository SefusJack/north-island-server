namespace NorthIsland.GameData.Marks;

/// <summary>One Mark.Dat entry: a quest or event mark.</summary>
/// <param name="Id">The mark id.</param>
/// <param name="FlagBit">The completion flag bit of the mark; 0 when it has none.</param>
/// <param name="Name">The mark name.</param>
/// <param name="HiddenInList">True when the quest list does not show the mark.</param>
/// <param name="Stars">Number of stars the quest pays when finished; 0 for none.</param>
public sealed record MarkRecord(
    ushort Id,
    ushort FlagBit,
    string Name,
    bool HiddenInList,
    byte Stars = 0);
