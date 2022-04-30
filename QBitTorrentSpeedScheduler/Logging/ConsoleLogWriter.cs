using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using QBitTorrentSpeedScheduler.Helper;

namespace QBitTorrentSpeedScheduler.Logging;

internal class ConsoleLogWriter : ILogWriter
{
    private readonly IOptions<GlobalLogChannelOptions> _formatOptions;

    public ConsoleLogWriter(IOptions<GlobalLogChannelOptions> formatOptions) => _formatOptions = formatOptions;

    public Task WriteAsync(string category, LogLevel logLevel, string message, Exception? exception)
    {
        if (Environment.UserInteractive)
        {
            var format = _formatOptions.Value;
            var timestamp = format.IncludeTimestamp ? $"{StringFormatter.CurrentTimestamp()} " : string.Empty;
            var source = format.IncludeCategory ? $"{category} " : string.Empty;
            var exceptionText = exception is not null ? $"{Environment.NewLine}{exception}" : null;
            var line = $"{timestamp}{source}{message}{exceptionText}";
            Console.WriteLine(line);
        }
        return Task.CompletedTask;
    }
}

internal static partial class Extensions
{
    private static IServiceCollection AddConsoleLogWriter(this IServiceCollection services) =>
        services.AddSingleton<ConsoleLogWriter>().AddSingleton<ILogWriter, ConsoleLogWriter>();
}