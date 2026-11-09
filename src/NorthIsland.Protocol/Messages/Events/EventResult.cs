namespace NorthIsland.Protocol.Messages.Events;

/// <summary>One event result as the client executes it.</summary>
/// <param name="Kind">Kind of result.</param>
/// <param name="Mode">Mode of the result.</param>
/// <param name="Target">Target id of the result.</param>
/// <param name="Option">Option byte of the result.</param>
/// <param name="Value">Numeric value of the result.</param>
/// <param name="DataId">Id of the data row the result refers to.</param>
public readonly record struct EventResult(
    byte Kind,
    byte Mode,
    ushort Target,
    byte Option,
    int Value,
    ushort DataId)
{
    /// <summary>Tells whether the client executes a result of this kind itself.</summary>
    /// <param name="kind">The result kind.</param>
    /// <returns>True for the kinds the client handles.</returns>
    public static bool IsClientKind(byte kind) => kind is 0 or 1 or 2 or 3 or 4 or 5 or 6 or 11 or 14 or 15;
}
