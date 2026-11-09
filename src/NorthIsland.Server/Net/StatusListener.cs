using System.Net;
using System.Net.Sockets;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace NorthIsland.Server.Net;

/// <summary>Background service that answers every connection on the status port with the server's status record and closes it.</summary>
/// <param name="options">The server options.</param>
/// <param name="logger">Logger for the handler's messages.</param>
public sealed class StatusListener(
    IOptions<ServerOptions> options,
    ILogger<StatusListener> logger) : BackgroundService
{
    /// <summary>The server options.</summary>
    private readonly ServerOptions _options = options.Value;

    /// <summary>Completed with the local endpoint once the listener is listening.</summary>
    private readonly TaskCompletionSource<IPEndPoint> _bound = new(TaskCreationOptions.RunContinuationsAsynchronously);

    /// <summary>Completes with the local endpoint once the listener is listening; it never completes when the status port is 0.</summary>
    public Task<IPEndPoint> Bound => _bound.Task;

    /// <summary>Builds the three-byte status record.</summary>
    /// <param name="serverId">The server id, sent low byte first.</param>
    /// <param name="status">The status byte.</param>
    /// <returns>The record.</returns>
    public static byte[] BuildRecord(ushort serverId, byte status) => [(byte)serverId, (byte)(serverId >> 8), status];

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (_options.StatusPort == 0)
            return;

        var listener = new TcpListener(IPAddress.Parse(_options.BindAddress), _options.StatusPort);
        listener.Start();
        _bound.TrySetResult((IPEndPoint)listener.LocalEndpoint);
        logger.LogInformation("Status server listening on {Endpoint}", listener.LocalEndpoint);
        var record = BuildRecord(_options.ServerId, _options.ServerStatus);

        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                TcpClient accepted;
                try
                {
                    accepted = await listener.AcceptTcpClientAsync(stoppingToken).ConfigureAwait(false);
                }
                catch (SocketException ex) when (!stoppingToken.IsCancellationRequested)
                {
                    await GameListener.AcceptFailedAsync(logger, ex, stoppingToken).ConfigureAwait(false);
                    continue;
                }
                using var client = accepted;
                try
                {
                    await client.GetStream().WriteAsync(record, stoppingToken).ConfigureAwait(false);
                    client.Client.Shutdown(SocketShutdown.Send);
                    logger.LogInformation("Status sent to {Remote}: server {Id} status {Status}",
                        client.Client.RemoteEndPoint, _options.ServerId, _options.ServerStatus);
                }
                catch (Exception ex) when (ex is IOException or SocketException)
                {
                    logger.LogDebug(ex, "Status client went away");
                }
            }
        }
        catch (OperationCanceledException)
        {
        }
        finally
        {
            listener.Stop();
        }
    }
}
