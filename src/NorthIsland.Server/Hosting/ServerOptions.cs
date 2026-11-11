namespace NorthIsland.Server.Hosting;

/// <summary>General server settings: connections, login, files, logging and spawn.</summary>
public sealed class ServerOptions
{
    /// <summary>The name of the configuration section.</summary>
    public const string Section = "Server";

    /// <summary>The IP address the game and status listeners bind to.</summary>
    public string BindAddress { get; set; } = "0.0.0.0";

    /// <summary>The TCP port of the game connection.</summary>
    public int GamePort { get; set; } = 6414;

    /// <summary>The TCP port of the status connection; 0 turns it off.</summary>
    public int StatusPort { get; set; } = 6416;

    /// <summary>The server id sent to the client in the server-selected packet and the status record.</summary>
    public ushort ServerId { get; set; } = 101;

    /// <summary>The server name sent to the client in the server-selected packet.</summary>
    public string ServerName { get; set; } = "Local";

    /// <summary>The flag sent with the server-selected packet.</summary>
    public bool ServerFlag { get; set; }

    /// <summary>The status byte of the status record.</summary>
    public byte ServerStatus { get; set; } = 1;

    /// <summary>The login versions the server accepts; when empty, the 2010 and 2014 versions.</summary>
    public ushort[] ClientVersions { get; set; } = [];

    /// <summary>Tells whether a login version is accepted.</summary>
    /// <param name="version">The version in the login packet.</param>
    /// <returns>True when the version is accepted.</returns>
    public bool AcceptsClientVersion(ushort version) =>
        ClientVersions.Length == 0 ? version is NorthIsland.Protocol.ClientVersions.Version2010 or NorthIsland.Protocol.ClientVersions.Version2014
            : ClientVersions.Contains(version);

    /// <summary>The expected size of the client's Item.dat; null for none.</summary>
    public long? ItemDatSize { get; set; }

    /// <summary>True to kick a login whose Item.dat size differs from <see cref="ItemDatSize"/>.</summary>
    public bool EnforceItemDatSize { get; set; }

    /// <summary>True to create an account when an unknown name logs in.</summary>
    public bool AutoRegister { get; set; } = true;

    /// <summary>True to tell the client to skip the cipher step of character creation.</summary>
    public bool SkipCreationCipher { get; set; } = true;

    /// <summary>What happens after a character is created.</summary>
    public AfterCreate AfterCreate { get; set; } = AfterCreate.EnterWorld;

    /// <summary>Where new characters start.</summary>
    public SpawnOptions Spawn { get; set; } = new();

    /// <summary>The directory of the client's data files or of the game, read only; empty to search for them (see <see cref="GameDataFolder"/>).</summary>
    public string GameDataDirectory { get; set; } = "";

    /// <summary>The directory of the server's own data (accounts and similar files).</summary>
    public string DataDirectory { get; set; } = "data";

    /// <summary>The directory of the log files.</summary>
    public string LogDirectory { get; set; } = "logs";

    /// <summary>The minutes between autosaves; 0 or less turns them off.</summary>
    public int AutosaveMinutes { get; set; } = 2;

    /// <summary>The file name of the event calendar, looked up in the data directory and then in the config folder.</summary>
    public string EventCalendarFile { get; set; } = "events.json";

    /// <summary>True to log every packet in both directions.</summary>
    public bool LogPackets { get; set; }

    /// <summary>Account names, separated by commas, that are made GMs when they log in; empty for none.</summary>
    public string GmAccounts { get; set; } = "";

    /// <summary>Tells whether an account name is listed in <see cref="GmAccounts"/>, ignoring case and spaces.</summary>
    /// <param name="name">The account name.</param>
    /// <returns>True when the account is listed.</returns>
    public bool ListsGm(string name) =>
        GmAccounts.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Contains(name, StringComparer.OrdinalIgnoreCase);
}
