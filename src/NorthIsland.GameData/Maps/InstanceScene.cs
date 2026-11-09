namespace NorthIsland.GameData.Maps;

/// <summary>A scene the client lists as an instance (duplicate mission), with the values of the instance window.</summary>
/// <param name="Number">The 1-based position in the instance list; selects the map ids of its copies.</param>
/// <param name="SceneId">The scene id of the instance.</param>
/// <param name="Name">The scene name.</param>
/// <param name="MaxPlayers">The largest number of players.</param>
/// <param name="GuildOnly">True when only guild members may enter.</param>
/// <param name="DoneMark">The mark that flags the instance as done; the title mark is this value minus 1.</param>
/// <param name="MinLevel">The level needed to enter.</param>
/// <param name="Minutes">The time limit in minutes.</param>
/// <param name="EntryX">X coordinate of the entry point on the instance maps.</param>
/// <param name="EntryY">Y coordinate of the entry point on the instance maps.</param>
public sealed record InstanceScene(
    int Number,
    ushort SceneId,
    string Name,
    byte MaxPlayers,
    bool GuildOnly,
    ushort DoneMark,
    ushort MinLevel,
    ushort Minutes,
    ushort EntryX,
    ushort EntryY)
{
    /// <summary>Map id of the first copy of the first instance.</summary>
    public const ushort FirstMapId = 61501;

    /// <summary>Number of map copies each instance has.</summary>
    public const int Copies = 10;

    /// <summary>Returns the map id of one copy of this instance.</summary>
    /// <param name="copy">The copy index, 0-based.</param>
    /// <returns>The map id.</returns>
    public ushort MapId(int copy) => (ushort)(FirstMapId + (Number - 1) * Copies + copy);

    /// <summary>The mark whose Mark.dat name titles the instance in the list.</summary>
    public ushort TitleMark => (ushort)(DoneMark - 1);
}
