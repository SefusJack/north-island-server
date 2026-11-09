namespace NorthIsland.Protocol.Messages.Player;

/// <summary>One skill of the player.</summary>
/// <param name="Key">The skill key.</param>
/// <param name="Data">The skill's data, <see cref="DataSize"/> bytes.</param>
public sealed record SkillEntry(
    ushort Key,
    byte[] Data)
{
    /// <summary>Number of data bytes of a skill.</summary>
    public const int DataSize = 5;
}
