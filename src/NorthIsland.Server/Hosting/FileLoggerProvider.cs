using System.Globalization;
using Microsoft.Extensions.Logging;

namespace NorthIsland.Server.Hosting;

/// <summary>Logger provider that appends to one log file per day, <c>server-yyyyMMdd.log</c>.</summary>
public sealed class FileLoggerProvider : ILoggerProvider
{
    /// <summary>The directory that holds the log files.</summary>
    private readonly string _directory;

    /// <summary>Serializes writing and the change of the day's file.</summary>
    private readonly Lock _lock = new();

    /// <summary>The writer of the current day's file; null before the first line and after disposal.</summary>
    private StreamWriter? _writer;

    /// <summary>The day <c>_writer</c> belongs to.</summary>
    private DateOnly _day;

    /// <summary>Creates the provider and the log directory.</summary>
    /// <param name="directory">The directory for the log files.</param>
    public FileLoggerProvider(
        string directory)
    {
        _directory = directory;
        Directory.CreateDirectory(directory);
    }

    public ILogger CreateLogger(string categoryName) => new FileLogger(this, categoryName);

    public void Dispose()
    {
        lock (_lock)
        {
            _writer?.Dispose();
            _writer = null;
        }
    }

    /// <summary>Writes a timestamped line, and the exception when there is one, to the day's file.</summary>
    /// <param name="category">The logger category.</param>
    /// <param name="level">The log level.</param>
    /// <param name="message">The formatted message.</param>
    /// <param name="exception">The exception to write below the line, or null.</param>
    private void Write(string category, LogLevel level, string message, Exception? exception)
    {
        var now = DateTime.Now;
        var line = string.Create(CultureInfo.InvariantCulture, $"{now:HH:mm:ss.fff} {LevelTag(level)}: {category}: {message}");
        lock (_lock)
        {
            var writer = WriterFor(DateOnly.FromDateTime(now));
            writer.WriteLine(line);
            if (exception is not null)
                writer.WriteLine(exception.ToString());
        }
    }

    /// <summary>Gives the writer for a day, opening a new append-mode file when the day has changed.</summary>
    /// <param name="day">The day of the line being written.</param>
    /// <returns>The writer for that day.</returns>
    private StreamWriter WriterFor(DateOnly day)
    {
        if (_writer is null || day != _day)
        {
            _writer?.Dispose();
            var path = Path.Combine(_directory, string.Create(CultureInfo.InvariantCulture, $"server-{day:yyyyMMdd}.log"));
            _writer = new StreamWriter(new FileStream(path, FileMode.Append, FileAccess.Write, FileShare.ReadWrite)) { AutoFlush = true };
            _day = day;
        }
        return _writer;
    }

    /// <summary>Gives the four-letter tag a level has in the log.</summary>
    /// <param name="level">The log level.</param>
    /// <returns>The tag.</returns>
    private static string LevelTag(LogLevel level) => level switch
    {
        LogLevel.Trace => "trce", // trace
        LogLevel.Debug => "dbug", // debug
        LogLevel.Information => "info", // information
        LogLevel.Warning => "warn", // warning
        LogLevel.Error => "fail", // error
        LogLevel.Critical => "crit", // critical
        _ => "none", // any other level
    };

    /// <summary>Logger of one category that hands its lines to the provider.</summary>
    /// <param name="provider">The provider that writes the lines.</param>
    /// <param name="category">The logger category.</param>
    private sealed class FileLogger(
        FileLoggerProvider provider,
        string category) : ILogger
    {
        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

        public bool IsEnabled(LogLevel logLevel) => logLevel != LogLevel.None;

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
            if (IsEnabled(logLevel))
                provider.Write(category, logLevel, formatter(state, exception), exception);
        }
    }
}
