namespace NorthIsland.Server.Hosting;

/// <summary>Resolves configured paths against the server's root directory.</summary>
public static class ServerPaths
{
    /// <summary>The file name that marks the root directory.</summary>
    private const string SolutionMarker = "NorthIslandServer.slnx";

    /// <summary>
    /// The server's root: the first directory, going up from the program's, that holds the solution file;
    /// the program's directory when none does.
    /// </summary>
    public static string Root { get; } = FindRoot();

    /// <summary>Makes a path absolute, relative paths being taken from the root.</summary>
    /// <param name="path">The configured path.</param>
    /// <returns>The absolute path.</returns>
    public static string Resolve(string path) => Path.IsPathRooted(path) ? path : Path.GetFullPath(Path.Combine(Root, path));

    /// <summary>Looks for the root directory.</summary>
    /// <returns>The directory that holds the solution file, or the program's directory.</returns>
    private static string FindRoot()
    {
        for (var dir = new DirectoryInfo(AppContext.BaseDirectory); dir is not null; dir = dir.Parent)
        {
            if (File.Exists(Path.Combine(dir.FullName, SolutionMarker)))
                return dir.FullName;
        }
        return AppContext.BaseDirectory;
    }
}
