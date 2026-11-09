using System.Collections.Concurrent;
using System.Collections.Immutable;

namespace NorthIsland.Server.Features.Travel;

/// <summary>The destinations of the transmit vehicles, found in the map data.</summary>
/// <param name="maps">The map data.</param>
public sealed class TransmitRoutes(
    MapData maps)
{
    /// <summary>The village names the village robot can go to, in the order of the client's list.</summary>
    public static readonly ImmutableArray<string> Villages =
        [
            "Kelan Village", "Welling Village", "Holy Village", "Fishing Village", "Mayan Tribe", "Long-eared Tribe", "Kyoto", "Calcutta", "Penguin Village",
            "Inchon", "Cairo", "Bangkok", "Inca", "Yamataikoku", "Athens", "Cornwall", "Domremy", "Persia", "Rome",
        ];

    /// <summary>The planet names the landing craft can go to, in the order of the client's list.</summary>
    public static readonly ImmutableArray<string> Planets = ["Earth", "The Moon", "The Mars"];

    /// <summary>The name of the seabed scene the submarine dives to.</summary>
    public const string Seabed = "Seabed";

    /// <summary>Gives the kind of vehicle an item is.</summary>
    /// <param name="itemId">The item id.</param>
    /// <returns>1 village robot, 2 submarine, 3 landing craft, 0 for any other item.</returns>
    public static int KindOf(int itemId) => itemId switch
    {
        48013 // UFO
            or 48031 => 1, // UFO Capsule
        48017 // Submarine
            or 48033 => 2, // Submarine Capsule
        48018 // Space Craft
            or 48034 => 3, // Space Craft Capsule
        _ => 0, // not a vehicle item
    };

    /// <summary>Counts the places a vehicle kind can go to.</summary>
    /// <param name="kind">The vehicle kind.</param>
    /// <returns>The number of places, 0 for an unknown kind.</returns>
    public static int Count(int kind) => kind switch
    {
        1 => Villages.Length, // village robot
        2 => 2, // submarine
        3 => Planets.Length, // landing craft
        _ => 0 // unknown kind: no places
    };

    /// <summary>The destinations already found, by vehicle kind and place number.</summary>
    private readonly ConcurrentDictionary<(int, int), DoorWarp?> _cache = new();

    /// <summary>Gives the destination of a place of a vehicle kind, found once and then remembered.</summary>
    /// <param name="kind">The vehicle kind.</param>
    /// <param name="index">The place number, 1-based.</param>
    /// <returns>The destination, or null when there is none.</returns>
    public DoorWarp? Route(int kind, int index) => index < 1 || index > Count(kind) ? null : _cache.GetOrAdd((kind, index), key => Find(key.Item1, key.Item2));

    /// <summary>Finds the destination of a place of a vehicle kind in the map data.</summary>
    /// <param name="kind">The vehicle kind.</param>
    /// <param name="index">The place number, 1-based.</param>
    /// <returns>The destination, or null when the data has none.</returns>
    private DoorWarp? Find(int kind, int index)
    {
        switch (kind)
        {
            case 1: // village robot
                return Landing(maps.SceneIdByName(Villages[index - 1]));
            case 3 when index == 1: // landing craft
                if (maps.SceneIdByName(Planets[1]) is { } moon && maps.Scene(moon)?.Conveys is [var home, ..])
                    return new DoorWarp(
                        home.TargetMapId,
                        (ushort)Math.Clamp(home.X, 0, ushort.MaxValue),
                        (ushort)Math.Clamp(home.Y, 0, ushort.MaxValue),
                        (ushort)home.Index);
                return null;
            case 3: // landing craft
                return Landing(maps.SceneIdByName(Planets[index - 1]));
            case 2 when index == 2: // submarine
                return Landing(maps.SceneIdByName(Seabed));
            case 2: // submarine
                if (maps.SceneIdByName(Seabed) is { } sea && maps.FirstDoorInto(sea) is { } entry
                    && maps.Scene(sea)?.Conveys.FirstOrDefault(c => c.TargetMapId == entry.From) is { } back)
                    return new DoorWarp(
                        back.TargetMapId,
                        (ushort)Math.Clamp(back.X, 0, ushort.MaxValue),
                        (ushort)Math.Clamp(back.Y, 0, ushort.MaxValue),
                        (ushort)back.Index);
                return null;
            default: // any other kind: no destination
                return null;
        }
    }

    /// <summary>Finds where a trip to a scene lands: the first door into its map, else the middle of the map.</summary>
    /// <param name="map">The map id of the scene, or null when the scene is unknown.</param>
    /// <returns>The destination, or null when there is none.</returns>
    private DoorWarp? Landing(ushort? map)
    {
        if (map is not { } id)
            return null;
        if (maps.FirstDoorInto(id) is { } door)
            return door.Warp;
        return maps.MapSize(id) is { } size ? new DoorWarp(id, (ushort)(size.Width / 2), (ushort)(size.Height / 2), 0) : null;
    }
}
