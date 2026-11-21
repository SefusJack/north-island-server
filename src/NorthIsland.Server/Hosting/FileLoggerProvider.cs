using System.Globalization;
using Microsoft.Extensions.Logging;

namespace NorthIsland.Server.Hosting;

/// <summary>
/// Logger provider that appends to one log file per day, <c>server-yyyyMMdd.log</c>, continued in <c>server-yyyyMMdd.2.log</c> and so on when a
/// file reaches its size cap; once the day's last part is full, the day's further lines are dropped. Control characters in a message are
/// escaped, so that text a client sent cannot fake log lines or terminal codes.
/// </summary>
public sealed class FileLoggerProvider : ILoggerProvider
{
    /// <summary>The default size cap of a log file, in megabytes.</summary>
    public const int DefaultFileMegabytes = 100;

    /// <summary>The default number of log files (parts) per day.</summary>
    public const int DefaultFilesPerDay = 10;

    /// <summary>The bytes in a megabyte.</summary>
    private const long BytesPerMegabyte = 1024 * 1024;

    /// <summary>The directory that holds the log files.</summary>
    private readonly string _directory;

    /// <summary>The size in bytes at which a file is closed and the next part begun.</summary>
    private readonly long _maxFileBytes;

    /// <summary>The most parts per day.</summary>
    private readonly int _filesPerDay;

    /// <summary>Serializes writing and the change of the day's file.</summary>
    private readonly Lock _lock = new();

    /// <summary>The writer of the current file; null before the first line, after disposal and once the day's last part is full.</summary>
    private StreamWriter? _writer;

    /// <summary>The day the current file belongs to.</summary>
    private DateOnly _day;

    /// <summary>The part of the day the current file is, from 1.</summary>
    private int _part;

    /// <summary>True once the day's last part is full and the day's further lines are dropped.</summary>
    private bool _dayFull;

    /// <summary>Creates the provider and the log directory.</summary>
    /// <param name="directory">The directory for the log files.</param>
    /// <param name="fileMegabytes">The size cap of a file in megabytes; 0 or less for the default.</param>
    /// <param name="filesPerDay">The most files per day; 0 or less for the default.</param>
    public FileLoggerProvider(
        string directory,
        int fileMegabytes = DefaultFileMegabytes,
        int filesPerDay = DefaultFilesPerDay)
    {
        _directory = directory;
        _maxFileBytes = (fileMegabytes > 0 ? fileMegabytes : DefaultFileMegabytes) * BytesPerMegabyte;
        _filesPerDay = filesPerDay > 0 ? filesPerDay : DefaultFilesPerDay;
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

    /// <summary>Writes a timestamped line, and the exception when there is one, to the day's file, unless the day's files are full.</summary>
    /// <param name="category">The logger category.</param>
    /// <param name="level">The log level.</param>
    /// <param name="message">The formatted message; its control characters are escaped.</param>
    /// <param name="exception">The exception to write below the line, or null.</param>
    private void Write(string category, LogLevel level, string message, Exception? exception)
    {
        var now = DateTime.Now;
        var line = string.Create(CultureInfo.InvariantCulture, $"{now:HH:mm:ss.fff} {LevelTag(level)}: {category}: {LogText.Printable(message)}");
        lock (_lock)
        {
            if (WriterFor(DateOnly.FromDateTime(now)) is not { } writer)
                return;
            writer.WriteLine(line);
            if (exception is not null)
                writer.WriteLine(exception.ToString());
        }
    }

    /// <summary>
    /// Gives the writer for a day: opens the day's first part when the day has changed, and the next part when the current one reached its size
    /// cap; writes a last line and gives null once the day's last part is full.
    /// </summary>
    /// <param name="day">The day of the line being written.</param>
    /// <returns>The writer, or null when the day's files are full.</returns>
    private StreamWriter? WriterFor(DateOnly day)
    {
        if (day != _day || (_writer is null && !_dayFull))
        {
            _writer?.Dispose();
            _day = day;
            _part = 1;
            _dayFull = false;
            _writer = Open(day, _part);
        }
        while (_writer is not null && _writer.BaseStream.Length >= _maxFileBytes)
        {
            if (_part >= _filesPerDay)
            {
                _writer.WriteLine(string.Create(CultureInfo.InvariantCulture, $"{DateTime.Now:HH:mm:ss.fff} warn: {nameof(FileLoggerProvider)}: "
                    + $"the day's {_filesPerDay} log file(s) are full; the day's further lines are dropped"));
                _writer.Dispose();
                _writer = null;
                _dayFull = true;
                break;
            }
            _writer.Dispose();
            _writer = Open(day, ++_part);
        }
        return _writer;
    }

    /// <summary>Opens a part of a day's log in append mode.</summary>
    /// <param name="day">The day.</param>
    /// <param name="part">The part, from 1; the first part has no number in its name.</param>
    /// <returns>The writer.</returns>
    private StreamWriter Open(DateOnly day, int part)
    {
        var name = part == 1 ? string.Create(CultureInfo.InvariantCulture, $"server-{day:yyyyMMdd}.log")
            : string.Create(CultureInfo.InvariantCulture, $"server-{day:yyyyMMdd}.{part}.log");
        var stream = new FileStream(Path.Combine(_directory, name), FileMode.Append, FileAccess.Write, FileShare.ReadWrite);
        return new StreamWriter(stream) { AutoFlush = true };
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
