using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;

namespace NorthIsland.Server.Hosting;

/// <summary>
/// The settings files, all in the <c>config/</c> folder next to the program so that one folder holds every setting. appsettings.json keeps the
/// essentials and the simple rates; the other files each hold their own keys of the same "Server" section, so the options classes do not know
/// which file a key is in. events.json and quiz.json are read from the same folder.
/// </summary>
public static class SettingsFiles
{
    /// <summary>The folder of the settings files, relative to the program.</summary>
    public const string Folder = "config";

    /// <summary>The paths of the files after appsettings.json, relative to the program.</summary>
    public static readonly string[] Names = ["config/battle.json", "config/progression.json", "config/world.json", "config/mall.json"];

    /// <summary>
    /// Moves the host's appsettings.json and appsettings.{Environment}.json into <see cref="Folder"/> and adds the other files right after
    /// appsettings.json, so appsettings.{Environment}.json, the environment and the command line can still override their keys.
    /// </summary>
    /// <param name="configuration">The configuration of the host, with appsettings.json already added.</param>
    public static void AddTo(IConfigurationBuilder configuration)
    {
        var sources = configuration.Sources;
        var hostFiles = sources.OfType<JsonConfigurationSource>()
            .Where(s => s.Path is { } p && p.StartsWith("appsettings.", StringComparison.OrdinalIgnoreCase) && !p.Contains('/', StringComparison.Ordinal))
            .ToList();
        var main = hostFiles.First(s => s.Path == "appsettings.json");
        foreach (var file in hostFiles)
            sources[sources.IndexOf(file)] = Like(main, $"{Folder}/{file.Path}", file.Optional);
        var at = sources.IndexOf(sources.OfType<JsonConfigurationSource>().First(s => s.Path == $"{Folder}/appsettings.json")) + 1;
        foreach (var name in Names)
            sources.Insert(at++, Like(main, name, main.Optional));
    }

    /// <summary>Makes a JSON source for another file, read the way the host reads appsettings.json.</summary>
    /// <param name="main">The host's appsettings.json source.</param>
    /// <param name="path">The path of the file, relative to the program.</param>
    /// <param name="optional">True when the file may be missing.</param>
    /// <returns>The source.</returns>
    private static JsonConfigurationSource Like(JsonConfigurationSource main, string path, bool optional) =>
        new() { Path = path, Optional = optional, ReloadOnChange = main.ReloadOnChange, FileProvider = main.FileProvider };
}
