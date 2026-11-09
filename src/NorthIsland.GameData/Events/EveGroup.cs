namespace NorthIsland.GameData.Events;

/// <summary>A named group of weighted member ids.</summary>
public sealed record EveGroup
{
    /// <summary>The group index within its section.</summary>
    public int Index { get; init; }

    /// <summary>The group name.</summary>
    public string Name { get; init; } = "";

    /// <summary>
    /// Byte read at offset 29 of the original record (name taken from that offset); its meaning is not established and nothing here uses it.
    /// </summary>
    public byte Unknown29 { get; init; }

    /// <summary>
    /// Word read at offset 30 of the original record (name taken from that offset); its meaning is not established and nothing here uses it.
    /// </summary>
    public ushort Unknown30 { get; init; }

    /// <summary>
    /// Byte read at offset 32 of the original record (name taken from that offset); its meaning is not established and nothing here uses it.
    /// </summary>
    public byte Unknown32 { get; init; }

    /// <summary>
    /// Byte read at offset 6 of the original record (name taken from that offset); its meaning is not established and nothing here uses it.
    /// </summary>
    public byte Unknown6 { get; init; }

    /// <summary>The members of the group.</summary>
    public IReadOnlyList<EveGroupMember> Members { get; init; } = [];
}
