namespace NorthIsland.GameData.Events;

/// <summary>The 22-byte record shared by event conditions and results.</summary>
/// <param name="Step">The step byte of the record.</param>
/// <param name="Kind">The record kind.</param>
/// <param name="SubKind">The kind-specific sub code.</param>
/// <param name="Arg1">The first word argument.</param>
/// <param name="Arg2">The second word argument.</param>
/// <param name="Arg3">The byte argument.</param>
/// <param name="Value">The first integer value.</param>
/// <param name="Link">The link byte of the record.</param>
/// <param name="Value2">The second integer value.</param>
/// <param name="Value3">The third integer value.</param>
public readonly record struct EveScriptRecord(
    byte Step,
    byte Kind,
    ushort SubKind,
    ushort Arg1,
    ushort Arg2,
    byte Arg3,
    int Value,
    byte Link,
    int Value2,
    int Value3)
{
    /// <summary>Size in bytes of the record in the file.</summary>
    public const int Size = 22;

    /// <summary>Returns the convey index when the record is a warp result.</summary>
    /// <returns>The convey index, or null when it is not a warp result.</returns>
    public int? WarpConvey() => Kind == 1 && SubKind == 3 && Arg1 != 0 ? Arg1 : null;

    /// <summary>Returns the speaker and talk id when the record is an NPC talk result.</summary>
    /// <returns>The speaking role index and talk id, or null when it is not an NPC talk result.</returns>
    public (int Role, int Talk)? NpcTalk() => Kind == 2 && Arg1 == 1 ? (SubKind, Arg2) : null;
}
