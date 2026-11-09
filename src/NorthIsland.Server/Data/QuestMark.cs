namespace NorthIsland.Server.Data;

/// <summary>A quest progress counter of a character.</summary>
public sealed class QuestMark
{
    /// <summary>Mark id.</summary>
    public ushort Id { get; set; }

    /// <summary>Counter value.</summary>
    public byte Count { get; set; }
}
