using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace NorthIsland.Server.Net;

/// <summary>Background service that accepts game connections and runs a session for each one, saving the account when it ends.</summary>
/// <param name="router">Dispatches the incoming packets of each session.</param>
/// <param name="store">The account store.</param>
/// <param name="world">The online sessions.</param>
/// <param name="options">The server options.</param>
/// <param name="loggers">Creates the loggers of the listener and its sessions.</param>
public sealed class GameListener(
    PacketRouter router,
    IAccountStore store,
    WorldState world,
    IOptions<ServerOptions> options,
    ILoggerFactory loggers) : BackgroundService
{
    /// <summary>The server options.</summary>
    private readonly ServerOptions _options = options.Value;

    /// <summary>The listener's logger.</summary>
    private readonly ILogger _logger = loggers.CreateLogger<GameListener>();

    /// <summary>The running session tasks, by session id.</summary>
    private readonly ConcurrentDictionary<int, Task> _sessions = new();

    /// <summary>The id given to the latest session.</summary>
    private int _lastSessionId;

    /// <summary>Completed with the local endpoint once the listener is listening.</summary>
    private readonly TaskCompletionSource<IPEndPoint> _bound = new(TaskCreationOptions.RunContinuationsAsynchronously);

    /// <summary>Completes with the local endpoint once the listener is listening.</summary>
    public Task<IPEndPoint> Bound => _bound.Task;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var listener = new TcpListener(IPAddress.Parse(_options.BindAddress), _options.GamePort);
        listener.Start();
        var endpoint = (IPEndPoint)listener.LocalEndpoint;
        _bound.TrySetResult(endpoint);
        _logger.LogInformation("Game server listening on {Endpoint}", endpoint);

        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                TcpClient client;
                try
                {
                    client = await listener.AcceptTcpClientAsync(stoppingToken).ConfigureAwait(false);
                }
                catch (SocketException ex) when (!stoppingToken.IsCancellationRequested)
                {
                    await AcceptFailedAsync(_logger, ex, stoppingToken).ConfigureAwait(false);
                    continue;
                }
                client.NoDelay = true;
                var session = new GameSession(
                    Interlocked.Increment(ref _lastSessionId),
                    client,
                    router,
                    loggers.CreateLogger<GameSession>(),
                    _options.LogPackets);
                _sessions[session.Id] = RunSessionAsync(session, stoppingToken);
            }
        }
        catch (OperationCanceledException)
        {
        }
        finally
        {
            listener.Stop();
            await Task.WhenAll(_sessions.Values).ConfigureAwait(false);
        }
    }

    /// <summary>Handles a failed accept: a client that left while queued is only logged, anything else is warned about and the loop waits 100 ms.</summary>
    /// <param name="logger">The logger to use.</param>
    /// <param name="ex">The socket error.</param>
    /// <param name="ct">Cancels the wait.</param>
    internal static async Task AcceptFailedAsync(ILogger logger, SocketException ex, CancellationToken ct)
    {
        if (ex.SocketErrorCode is SocketError.ConnectionReset or SocketError.ConnectionAborted)
        {
            logger.LogDebug("Accept: client went away while queued ({Error})", ex.SocketErrorCode);
            return;
        }
        logger.LogWarning(ex, "Accept failed ({Error}); still listening", ex.SocketErrorCode);
        await Task.Delay(TimeSpan.FromMilliseconds(100), ct).ConfigureAwait(false);
    }

    /// <summary>Runs a session and, when it ends, aborts its trades, removes it from the world and saves its account.</summary>
    /// <param name="session">The session to run.</param>
    /// <param name="ct">Cancels the session.</param>
    private async Task RunSessionAsync(GameSession session, CancellationToken ct)
    {
        await using (session.ConfigureAwait(false))
        {
            try
            {
                await session.RunAsync(ct).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[{Session}] session crashed", session.Id);
            }
        }

        TradeHandlers.Abort(session);
        PetTradeHandlers.Abort(session);
        session.Stalls.Items = null;
        session.Stalls.Pets = null;
        session.Stalls.Utensils = null;
        world.Leave(session);

        if (session.Account is { } account && session.ActiveCharacter is { } character)
        {
            try
            {
                await store.SaveAsync(account, CancellationToken.None).ConfigureAwait(false);
                _logger.LogInformation("[{Session}] saved {Name} at map {Map} ({X}, {Y})",
                    session.Id, character.Name, character.MapId, character.X, character.Y);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[{Session}] saving {Account} failed", session.Id, account.Name);
            }
        }
        _sessions.TryRemove(session.Id, out _);
    }
}
