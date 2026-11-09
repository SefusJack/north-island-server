using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace NorthIsland.Server.Hosting;

/// <summary>Background service that stops the server, saving everyone online, when a named Windows event is signaled.</summary>
/// <param name="lifetime">Stops the application.</param>
/// <param name="logger">Logger for the handler's messages.</param>
public sealed class ShutdownSignal(
    IHostApplicationLifetime lifetime,
    ILogger<ShutdownSignal> logger) : BackgroundService
{
    /// <summary>Gives the name of the stop event of a server process.</summary>
    /// <param name="processId">The process id of the server.</param>
    /// <returns>The event name.</returns>
    public static string EventName(int processId) => $"NorthIslandServer-Stop-{processId}";

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!OperatingSystem.IsWindows())
            return;

        using var signal = new EventWaitHandle(false, EventResetMode.ManualReset, EventName(Environment.ProcessId));
        var fired = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var registration = ThreadPool.RegisterWaitForSingleObject(signal, (_, _) => fired.TrySetResult(), null, Timeout.Infinite, executeOnlyOnce: true);
        try
        {
            await fired.Task.WaitAsync(stoppingToken).ConfigureAwait(false);
            logger.LogInformation("Stop requested; shutting down and saving everyone online");
            lifetime.StopApplication();
        }
        catch (OperationCanceledException)
        {
        }
        finally
        {
            registration.Unregister(null);
        }
    }
}
