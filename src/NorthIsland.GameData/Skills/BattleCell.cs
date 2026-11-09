namespace NorthIsland.GameData.Skills;

/// <summary>A cell of the battle grid.</summary>
/// <param name="Row">Grid row 1..4; rows 1-2 are on one side of the field and rows 3-4 on the other.</param>
/// <param name="Col">Column 1..4 within the row.</param>
public readonly record struct BattleCell(
    int Row,
    int Col);
