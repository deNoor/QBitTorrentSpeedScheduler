using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using QBitTorrentSpeedScheduler.Config;
using QBitTorrentSpeedScheduler.Helper;

namespace QBitTorrentSpeedScheduler.Logging;

internal class FileLogWriter : ILogWriter, IDisposable
{
    private static readonly ICollection<LogLevel> _treatAsError = new HashSet<LogLevel>
    {
        LogLevel.Warning, LogLevel.Error, LogLevel.Critical,
    };

    private readonly ConsoleLogWriter _consoleLog;
    private readonly IOptions<GlobalLogChannelOptions> _formatOptions;
    private readonly IDisposable? _settingsChangeListener;

    private volatile string? _filePath;
    private volatile bool _errorsOnly;

    public FileLogWriter(
        ConsoleLogWriter consoleLog,
        IOptions<GlobalLogChannelOptions> formatOptions,
        IOptionsMonitor<Settings> optionsMonitor)
    {
        _consoleLog = consoleLog;
        _formatOptions = formatOptions;
        UpdateFilePathSettings(optionsMonitor.CurrentValue);
        _settingsChangeListener = optionsMonitor.OnChange(settings => Task.Run(() => UpdateFilePathSettings(settings)));
    }

    public void Dispose() => _settingsChangeListener?.Dispose();

    public async Task WriteAsync(string category, LogLevel logLevel, string message, Exception? exception)
    {
        if (_errorsOnly && !_treatAsError.Contains(logLevel))
        {
            return;
        }

        var filePath = _filePath;
        if (filePath is not null)
        {
            try
            {
                var format = _formatOptions.Value;
                var timestamp = format.IncludeTimestamp ? $"{StringFormatter.CurrentTimestamp()} " : string.Empty;
                var exceptionText = exception is not null ? $"{Environment.NewLine}{exception.Message}" : string.Empty;
                var line = $"{timestamp}{message}{exceptionText}{Environment.NewLine}";
                await File.AppendAllTextAsync(filePath, line);
            }
            catch (TaskCanceledException)
            {
            }
            catch (Exception e)
            {
                await _consoleLog.WriteAsync(nameof(FileLogWriter), LogLevel.Critical, "file logging failed with error: ", e);
            }
        }
    }

    internal void UpdateFilePathSettings(Settings settings)
    {
        var logFile = settings.LogFile ?? new() { Enabled = false, };
        string? filePath = null;
        if (logFile.Enabled)
        {
            var folder = logFile.Folder;
            if (string.IsNullOrWhiteSpace(folder))
            {
                folder = AppContext.BaseDirectory;
            }
            if (Directory.Exists(Directory.GetDirectoryRoot(folder))) // check disk letter exists.
            {
                if (!Directory.Exists(folder))
                {
                    Directory.CreateDirectory(folder); // File.Append can't create full path so let's help.
                }
                filePath = Path.Combine(folder, LogFile.FileName);
            }
        }
        _errorsOnly = logFile.ErrorsOnly;
        _filePath = filePath;
    }
}

internal static partial class Extensions
{
    private static IServiceCollection AddFileLogWriter(this IServiceCollection services) =>
        services.AddSingleton<FileLogWriter>().AddSingleton<ILogWriter, FileLogWriter>();
}
