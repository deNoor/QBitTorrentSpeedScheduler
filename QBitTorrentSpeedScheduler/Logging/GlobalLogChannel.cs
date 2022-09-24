using System;
using System.Collections.Generic;
using System.Threading.Channels;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace QBitTorrentSpeedScheduler.Logging;

internal class GlobalLogChannel
{
    private readonly Channel<(string Category, LogLevel LogLevel, string Message, Exception? Exception)> _channel;
    private readonly IEnumerable<ILogWriter> _logWriters;

    public GlobalLogChannel(IEnumerable<ILogWriter> logWriters)
    {
        _channel = Channel.CreateUnbounded<(string Category, LogLevel LogLevel, string Message, Exception? Exception)>(
            new() { SingleReader = true, AllowSynchronousContinuations = true, });
        BeginChannelRead(); // here is our single reader.
        _logWriters = logWriters;
    }

    public void Log(string category, LogLevel logLevel, string message, Exception? exception) =>
        Task.Run(async () => await _channel.Writer.WriteAsync((category, logLevel, message, exception)));

    private void BeginChannelRead() =>
        Task.Run(
            async () =>
            {
                await foreach (var entry in _channel.Reader.ReadAllAsync())
                {
                    var (category, logLevel, message, exception) = entry;
                    foreach (var logWriter in _logWriters)
                    {
                        await logWriter.WriteAsync(category, logLevel, message, exception);
                    }
                }
            });
}

internal static partial class Extensions
{
    private static IServiceCollection AddGlobalLogChannel(this IServiceCollection services) => services.AddSingleton<GlobalLogChannel>();
}
