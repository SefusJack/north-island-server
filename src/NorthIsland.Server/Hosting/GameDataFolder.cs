namespace NorthIsland.Server.Hosting;

/// <summary>
/// Finds the client's data files the way the client does: the folder given with <c>--data</c> or in the settings, otherwise the server's own folder,
/// then the default install.
/// </summary>
public static class GameDataFolder
{
    /// <summary>A file that every folder of game data holds.</summary>
    private const string Marker = "Item.dat";

    /// <summary>The name of the data folder inside the game's folder.</summary>
    private const string DataFolder = "data";

    /// <summary>The name of the game's folder under Program Files (x86), Windows only.</summary>
    private const string InstallName = "Wonderland Online";

    /// <summary>Lists the places to look in.</summary>
    /// <param name="given">The folder given by the user, if any; then it is the only place.</param>
    /// <returns>The places, the first choice first.</returns>
    public static IReadOnlyList<string> Places(string? given)
    {
        if (!string.IsNullOrWhiteSpace(given))
            return [ServerPaths.Resolve(given)];
        List<string> places = [AppContext.BaseDirectory, ServerPaths.Root];
        var programs = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);
        if (programs.Length > 0)
            places.Add(Path.Combine(programs, InstallName));
        return places;
    }

    /// <summary>Picks the first place that holds the game data, either directly or in its data folder.</summary>
    /// <param name="places">The places to look in.</param>
    /// <returns>The folder of the data files, or null when none has them.</returns>
    public static string? Find(IEnumerable<string> places) =>
        places.SelectMany(p => new[] { p, Path.Combine(p, DataFolder) }).FirstOrDefault(d => File.Exists(Path.Combine(d, Marker)));
}
