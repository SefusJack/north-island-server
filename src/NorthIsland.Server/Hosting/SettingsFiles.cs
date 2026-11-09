using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;

namespace NorthIsland.Server.Hosting;

/// <summary>
/// The settings files in <c>config/</c>. Each holds its own keys of the same "Server" section, so the options classes do not know which file a key is in;
/// appsettings.json keeps the essentials and the simple rates.
/// </summary>
public static class SettingsFiles
{
    /// <summary>The paths of the files, relative to appsettings.json.</summary>
    public static readonly string[] Names = ["config/battle.json", "config/progression.json", "config/world.json", "config/mall.json"];

    /// <summary>
    /// Adds the files right after appsettings.json, so appsettings.{Environment}.json, the environment and the command line can still override their keys.
    /// </summary>
    /// <param name="configuration">The configuration of the host, with appsettings.json already added.</param>
    public static void AddTo(IConfigurationBuilder configuration)
    {
        var sources = configuration.Sources;
        var main = sources.OfType<JsonConfigurationSource>().First(s => s.Path == "appsettings.json");
        var at = sources.IndexOf(main) + 1;
        foreach (var name in Names)
        {
            sources.Insert(
                at++,
                new JsonConfigurationSource { Path = name, Optional = main.Optional, ReloadOnChange = main.ReloadOnChange, FileProvider = main.FileProvider });
        }
    }
}
