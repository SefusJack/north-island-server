using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

// The settings files sit next to the program, wherever it is started from.
var builder = Host.CreateApplicationBuilder(new HostApplicationBuilderSettings { Args = args, ContentRootPath = AppContext.BaseDirectory });

SettingsFiles.AddTo(builder.Configuration);

var gameDataKey = $"{ServerOptions.Section}:{nameof(ServerOptions.GameDataDirectory)}";

builder.Configuration.AddCommandLine(args, new Dictionary<string, string> { ["--data"] = gameDataKey });

var gameDataPlaces = GameDataFolder.Places(builder.Configuration[gameDataKey]);

var gameData = GameDataFolder.Find(gameDataPlaces);

if (gameData is null)
{
    await Console.Error.WriteLineAsync(
        $"No game data found. Looked in: {string.Join("; ", gameDataPlaces)}. Start with --data <folder of the game> or set Server:GameDataDirectory.")
        .ConfigureAwait(false);
    return 1;
}

builder.Configuration.AddInMemoryCollection([new(gameDataKey, gameData)]);

builder.Logging.AddSimpleConsole(o =>
{
    o.SingleLine = true;
    o.TimestampFormat = "HH:mm:ss.fff ";
});

var logDirectory = ServerPaths.Resolve(builder.Configuration[$"{ServerOptions.Section}:{nameof(ServerOptions.LogDirectory)}"] ?? "logs");

builder.Logging.AddProvider(new FileLoggerProvider(logDirectory));

builder.Services.AddGameServer(builder.Configuration.GetSection(ServerOptions.Section));

await builder.Build().RunAsync().ConfigureAwait(false);

return 0;
