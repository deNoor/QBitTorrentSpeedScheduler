using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace QBitTorrentSpeedScheduler.Logging;

internal class ConsoleLogWriter : ILogWriter
{
    public Task WriteAsync(LogLevel logLevel, string message, Exception? exception)
    {
        if (Environment.UserInteractive)
        {
            Console.WriteLine($"{message}{(exception is not null ? $"{Environment.NewLine}{exception}" : null)}");
        }
        return Task.CompletedTask;
    }
}

internal static partial class Extensions
{
    private static IServiceCollection AddConsoleLogWriter(this IServiceCollection services) =>
        services.AddSingleton<ILogWriter, ConsoleLogWriter>();
}