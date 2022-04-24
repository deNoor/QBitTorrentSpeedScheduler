using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Channels;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using QBitTorrentSpeedScheduler.Config;

namespace QBitTorrentSpeedScheduler.Service
{
    internal class GlobalLogChannel : IDisposable
    {
        private readonly Channel<ICollection<string>> _linesChannel;
        private readonly IDisposable? _settingsChangeListener;

        private volatile string? _filePath;
        private volatile bool _errorsOnly;

        public GlobalLogChannel(IOptionsMonitor<Settings> optionsMonitor)
        {
            _linesChannel = Channel.CreateUnbounded<ICollection<string>>(
                new() { SingleReader = true, AllowSynchronousContinuations = true, });
            UpdateFilePathSettings(optionsMonitor.CurrentValue);
            var _ = StartChannelReader(); // here is our single reader.
            _settingsChangeListener = optionsMonitor.OnChange(settings => Task.Run(() => UpdateFilePathSettings(settings)));
        }

        public void LogError(params string[] logLines) => Log(logLines);

        public void LogInfo(params string[] logLines)
        {
            if (!_errorsOnly)
            {
                Log(logLines);
            }
        }

        public void Dispose() => _settingsChangeListener?.Dispose();

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

        private static ICollection<string> FormatLines(IEnumerable<string> lines) =>
            lines.Select(x => $@"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {x}").ToList();

        private static void WriteToConsole(IEnumerable<string> logLines)
        {
            if (Environment.UserInteractive)
            {
                foreach (var logLine in logLines)
                {
                    Console.WriteLine(logLine);
                }
            }
        }

        private Task StartChannelReader() =>
            Task.Run(
                async () =>
                {
                    await foreach (var lines in _linesChannel.Reader.ReadAllAsync())
                    {
                        var logLines = FormatLines(lines);
                        WriteToConsole(logLines);
                        var filePath = _filePath;
                        if (filePath is not null)
                        {
                            try
                            {
                                await File.AppendAllLinesAsync(filePath, logLines);
                            }
                            catch (TaskCanceledException)
                            {
                            }
                            catch (Exception e)
                            {
                                WriteToConsole(FormatLines(new[] { "file logging failed with error:", e.Message, }));
                            }
                        }
                    }
                });

        private void Log(params string[] lines) => Task.Run(async () => await _linesChannel.Writer.WriteAsync(lines));
    }

    internal static partial class Extensions
    {
        public static IServiceCollection AddLogChannel(this IServiceCollection services) => services.AddSingleton<GlobalLogChannel>();
    }
}