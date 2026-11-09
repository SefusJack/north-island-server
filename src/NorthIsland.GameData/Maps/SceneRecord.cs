namespace NorthIsland.GameData.Maps;

/// <summary>One SceneData.Dat entry: a map or a battle background.</summary>
/// <param name="Id">The scene id.</param>
/// <param name="Name">The scene name.</param>
/// <param name="Type">The scene type byte.</param>
/// <param name="Flags">Flag bits of the scene.</param>
public sealed record SceneRecord(
    ushort Id,
    string Name,
    byte Type,
    byte Flags = 0);
